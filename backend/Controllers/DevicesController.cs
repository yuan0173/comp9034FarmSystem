using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP9034.Backend.Data;
using COMP9034.Backend.Models;

namespace COMP9034.Backend.Controllers
{
    /// <summary>
    /// Device management API controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(ApplicationDbContext context, ILogger<DevicesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all devices list
        /// </summary>
        /// <param name="limit">Limit return count</param>
        /// <param name="offset">Offset</param>
        /// <param name="type">Device type filter</param>
        /// <param name="status">Device status filter</param>
        /// <param name="location">Location search</param>
        /// <returns>Devices list</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> GetDevices(
            [FromQuery] int? limit = null,
            [FromQuery] int offset = 0,
            [FromQuery] string? type = null,
            [FromQuery] string? status = null,
            [FromQuery] string? location = null)
        {
            try
            {
                var query = _context.Devices.AsQueryable();

                // Apply type filter
                if (!string.IsNullOrWhiteSpace(type))
                {
                    query = query.Where(d => d.Type == type);
                }

                // Apply status filter
                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(d => d.Status == status);
                }

                // Apply location search
                if (!string.IsNullOrWhiteSpace(location))
                {
                    query = query.Where(d => d.Location != null && d.Location.Contains(location));
                }

                // Only return active devices
                query = query.Where(d => d.IsActive);

                // Sort
                query = query.OrderBy(d => d.Id);

                // Apply pagination
                if (offset > 0)
                {
                    query = query.Skip(offset);
                }

                if (limit.HasValue)
                {
                    query = query.Take(limit.Value);
                }

                var devices = await query.ToListAsync();

                _logger.LogInformation($"Returning {devices.Count} device records");
                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting devices list");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get specific device by ID
        /// </summary>
        /// <param name="id">Device ID</param>
        /// <returns>Device information</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> GetDevice(int id)
        {
            try
            {
                var device = await _context.Devices
                    .Include(d => d.Events.OrderByDescending(e => e.TimeStamp).Take(10))
                    .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);

                if (device == null)
                {
                    return NotFound(new { message = $"Device with ID {id} not found" });
                }

                return Ok(device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting device ID:{id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new device
        /// </summary>
        /// <param name="device">Device information</param>
        /// <returns>Created device information</returns>
        [HttpPost]
        public async Task<ActionResult<Device>> PostDevice(Device device)
        {
            try
            {
                // Verify device name doesn't already exist
                if (await _context.Devices.AnyAsync(d => d.Name == device.Name && d.IsActive))
                {
                    return BadRequest(new { message = $"Device name '{device.Name}' already exists" });
                }

                // Validate device type
                var validTypes = new[] { "biometric", "terminal", "card_reader" };
                if (!validTypes.Contains(device.Type))
                {
                    return BadRequest(new { message = $"Invalid device type: {device.Type}. Valid types: {string.Join(", ", validTypes)}" });
                }

                // Validate device status
                var validStatuses = new[] { "active", "inactive", "maintenance" };
                if (!validStatuses.Contains(device.Status))
                {
                    device.Status = "active"; // Default status
                }

                // Validate IP address format (if provided)
                if (!string.IsNullOrWhiteSpace(device.IpAddress))
                {
                    if (!System.Net.IPAddress.TryParse(device.IpAddress, out _))
                    {
                        return BadRequest(new { message = $"Invalid IP address format: {device.IpAddress}" });
                    }
                }

                device.CreatedAt = DateTime.UtcNow;
                device.UpdatedAt = DateTime.UtcNow;
                device.IsActive = true;

                _context.Devices.Add(device);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created new device: ID={device.Id}, Name={device.Name}, Type={device.Type}");
                return CreatedAtAction(nameof(GetDevice), new { id = device.Id }, device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating device");
                return StatusCode(500, new { message = "Failed to create device", error = ex.Message });
            }
        }

        /// <summary>
        /// Update device information
        /// </summary>
        /// <param name="id">Device ID</param>
        /// <param name="device">Updated device information</param>
        /// <returns>Update result</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDevice(int id, Device device)
        {
            if (id != device.Id)
            {
                return BadRequest(new { message = "ID in path does not match ID in request body" });
            }

            try
            {
                var existingDevice = await _context.Devices.FindAsync(id);
                if (existingDevice == null)
                {
                    return NotFound(new { message = $"Device with ID {id} not found" });
                }

                // Verify device name doesn't conflict with other devices
                if (await _context.Devices.AnyAsync(d => d.Name == device.Name && d.Id != id && d.IsActive))
                {
                    return BadRequest(new { message = $"Device name '{device.Name}' already exists" });
                }

                // Validate device type
                var validTypes = new[] { "biometric", "terminal", "card_reader" };
                if (!validTypes.Contains(device.Type))
                {
                    return BadRequest(new { message = $"Invalid device type: {device.Type}" });
                }

                // Validate device status
                var validStatuses = new[] { "active", "inactive", "maintenance" };
                if (!validStatuses.Contains(device.Status))
                {
                    return BadRequest(new { message = $"Invalid device status: {device.Status}" });
                }

                // Validate IP address format (if provided)
                if (!string.IsNullOrWhiteSpace(device.IpAddress))
                {
                    if (!System.Net.IPAddress.TryParse(device.IpAddress, out _))
                    {
                        return BadRequest(new { message = $"Invalid IP address format: {device.IpAddress}" });
                    }
                }

                // Update fields
                existingDevice.Name = device.Name;
                existingDevice.Type = device.Type;
                existingDevice.Location = device.Location;
                existingDevice.Status = device.Status;
                existingDevice.IpAddress = device.IpAddress;
                existingDevice.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated device: ID={id}");
                return Ok(new { message = "Device information updated successfully", device = existingDevice });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating device ID:{id}");
                return StatusCode(500, new { message = "Failed to update device", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete device (soft delete)
        /// </summary>
        /// <param name="id">Device ID</param>
        /// <returns>Delete result</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            try
            {
                var device = await _context.Devices.FindAsync(id);
                if (device == null)
                {
                    return NotFound(new { message = $"Device with ID {id} not found" });
                }

                // Soft delete
                device.IsActive = false;
                device.Status = "inactive";
                device.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted device: ID={id}");
                return Ok(new { message = "Device deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting device ID:{id}");
                return StatusCode(500, new { message = "Failed to delete device", error = ex.Message });
            }
        }

        /// <summary>
        /// Update device status
        /// </summary>
        /// <param name="id">Device ID</param>
        /// <param name="status">New status</param>
        /// <returns>Update result</returns>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateDeviceStatus(int id, [FromBody] string status)
        {
            try
            {
                var device = await _context.Devices.FindAsync(id);
                if (device == null)
                {
                    return NotFound(new { message = $"Device with ID {id} not found" });
                }

                // Validate status
                var validStatuses = new[] { "active", "inactive", "maintenance" };
                if (!validStatuses.Contains(status))
                {
                    return BadRequest(new { message = $"Invalid device status: {status}. Valid statuses: {string.Join(", ", validStatuses)}" });
                }

                device.Status = status;
                device.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated device status: ID={id}, Status={status}");
                return Ok(new { message = "Device status updated successfully", device = new { device.Id, device.Name, device.Status } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating device status: ID={id}");
                return StatusCode(500, new { message = "Failed to update device status", error = ex.Message });
            }
        }

        /// <summary>
        /// Get device statistics
        /// </summary>
        /// <returns>Device statistics</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult> GetDeviceStatistics()
        {
            try
            {
                var totalDevices = await _context.Devices.CountAsync(d => d.IsActive);
                var activeDevices = await _context.Devices.CountAsync(d => d.IsActive && d.Status == "active");
                var inactiveDevices = await _context.Devices.CountAsync(d => d.IsActive && d.Status == "inactive");
                var maintenanceDevices = await _context.Devices.CountAsync(d => d.IsActive && d.Status == "maintenance");

                var devicesByType = await _context.Devices
                    .Where(d => d.IsActive)
                    .GroupBy(d => d.Type)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToListAsync();

                var statistics = new
                {
                    total = totalDevices,
                    active = activeDevices,
                    inactive = inactiveDevices,
                    maintenance = maintenanceDevices,
                    byType = devicesByType
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting device statistics");
                return StatusCode(500, new { message = "Failed to get device statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// Test device connection
        /// </summary>
        /// <param name="id">Device ID</param>
        /// <returns>Connection test result</returns>
        [HttpPost("{id}/test")]
        public async Task<ActionResult> TestDeviceConnection(int id)
        {
            try
            {
                var device = await _context.Devices.FindAsync(id);
                if (device == null)
                {
                    return NotFound(new { message = $"Device with ID {id} not found" });
                }

                if (string.IsNullOrWhiteSpace(device.IpAddress))
                {
                    return BadRequest(new { message = "Device has no IP address configured" });
                }

                // Simulate connection test (in actual implementation, you can ping the device or send test request)
                var isConnected = await SimulateConnectionTest(device.IpAddress);

                var result = new
                {
                    deviceId = device.Id,
                    deviceName = device.Name,
                    ipAddress = device.IpAddress,
                    isConnected = isConnected,
                    testTime = DateTime.UtcNow,
                    message = isConnected ? "Device connection normal" : "Device connection failed"
                };

                _logger.LogInformation($"Device connection test: ID={id}, IP={device.IpAddress}, Result={isConnected}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while testing device connection: ID={id}");
                return StatusCode(500, new { message = "Connection test failed", error = ex.Message });
            }
        }

        /// <summary>
        /// Simulate device connection test
        /// </summary>
        /// <param name="ipAddress">IP address</param>
        /// <returns>Whether connection is successful</returns>
        private async Task<bool> SimulateConnectionTest(string ipAddress)
        {
            try
            {
                // Simple ping test simulation
                // In actual implementation, you can use System.Net.NetworkInformation.Ping
                await Task.Delay(100); // Simulate network delay
                
                // Simple logic: consider 192.168.x.x as internal network connectable, others random
                if (ipAddress.StartsWith("192.168."))
                {
                    return true;
                }
                
                return new Random().NextDouble() > 0.3; // 70% success rate
            }
            catch
            {
                return false;
            }
        }
    }
}