using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP9034.Backend.Data;
using COMP9034.Backend.Models;
using System.Security.Cryptography;
using System.Text;

namespace COMP9034.Backend.Controllers
{
    /// <summary>
    /// Biometric data management API controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BiometricController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BiometricController> _logger;

        public BiometricController(ApplicationDbContext context, ILogger<BiometricController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all biometric data list
        /// </summary>
        /// <param name="limit">Limit return count</param>
        /// <param name="offset">Offset</param>
        /// <param name="staffId">Staff ID filter</param>
        /// <param name="biometricType">Biometric type filter</param>
        /// <returns>Biometric data list</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetBiometricData(
            [FromQuery] int? limit = null,
            [FromQuery] int offset = 0,
            [FromQuery] int? staffId = null,
            [FromQuery] string? biometricType = null)
        {
            try
            {
                var query = _context.BiometricData
                    .Include(b => b.Staff)
                    .AsQueryable();

                // Apply staff filter
                if (staffId.HasValue)
                {
                    query = query.Where(b => b.StaffId == staffId.Value);
                }

                // Apply biometric type filter
                if (!string.IsNullOrWhiteSpace(biometricType))
                {
                    query = query.Where(b => b.BiometricType == biometricType);
                }

                // Only return active data
                query = query.Where(b => b.IsActive);

                // Sort
                query = query.OrderByDescending(b => b.CreatedAt);

                // Apply pagination
                if (offset > 0)
                {
                    query = query.Skip(offset);
                }

                if (limit.HasValue)
                {
                    query = query.Take(limit.Value);
                }

                var biometricData = await query.ToListAsync();

                // Hide sensitive template data when returning, only return summary information
                var result = biometricData.Select(b => new
                {
                    b.Id,
                    b.StaffId,
                    Staff = new { b.Staff.Id, b.Staff.Name },
                    b.BiometricType,
                    HasTemplateData = !string.IsNullOrEmpty(b.TemplateData),
                    TemplateSize = b.TemplateData?.Length ?? 0,
                    b.IsActive,
                    b.CreatedAt,
                    b.UpdatedAt
                });

                _logger.LogInformation($"Returning {biometricData.Count} biometric records");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting biometric data list");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get specific biometric data by ID
        /// </summary>
        /// <param name="id">Biometric data ID</param>
        /// <returns>Biometric data information</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetBiometricData(int id)
        {
            try
            {
                var biometricData = await _context.BiometricData
                    .Include(b => b.Staff)
                    .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);

                if (biometricData == null)
                {
                    return NotFound(new { message = $"Biometric data with ID {id} not found" });
                }

                // Hide sensitive template data when returning
                var result = new
                {
                    biometricData.Id,
                    biometricData.StaffId,
                    Staff = new { biometricData.Staff.Id, biometricData.Staff.Name },
                    biometricData.BiometricType,
                    HasTemplateData = !string.IsNullOrEmpty(biometricData.TemplateData),
                    TemplateSize = biometricData.TemplateData?.Length ?? 0,
                    biometricData.IsActive,
                    biometricData.CreatedAt,
                    biometricData.UpdatedAt
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting biometric data ID:{id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new biometric data
        /// </summary>
        /// <param name="biometricData">Biometric data information</param>
        /// <returns>Created biometric data information</returns>
        [HttpPost]
        public async Task<ActionResult<object>> PostBiometricData(BiometricData biometricData)
        {
            try
            {
                // Verify staff exists and is active
                var staff = await _context.Staff.FindAsync(biometricData.StaffId);
                if (staff == null || !staff.IsActive)
                {
                    return BadRequest(new { message = $"Staff ID {biometricData.StaffId} does not exist or has been disabled" });
                }

                // Validate biometric type
                var validTypes = new[] { "fingerprint", "face", "iris", "voice" };
                if (!validTypes.Contains(biometricData.BiometricType))
                {
                    return BadRequest(new { message = $"Invalid biometric type: {biometricData.BiometricType}. Valid types: {string.Join(", ", validTypes)}" });
                }

                // Check if the staff already has biometric data of the same type
                var existingData = await _context.BiometricData
                    .FirstOrDefaultAsync(b => b.StaffId == biometricData.StaffId && 
                                            b.BiometricType == biometricData.BiometricType && 
                                            b.IsActive);

                if (existingData != null)
                {
                    return BadRequest(new { message = $"Staff {biometricData.StaffId} already has {biometricData.BiometricType} type biometric data" });
                }

                // Validate template data
                if (string.IsNullOrWhiteSpace(biometricData.TemplateData))
                {
                    return BadRequest(new { message = "Biometric template data cannot be empty" });
                }

                // Encrypt and store template data
                biometricData.TemplateData = EncryptTemplateData(biometricData.TemplateData);
                biometricData.CreatedAt = DateTime.UtcNow;
                biometricData.UpdatedAt = DateTime.UtcNow;
                biometricData.IsActive = true;

                _context.BiometricData.Add(biometricData);
                await _context.SaveChangesAsync();

                // Re-fetch record with related data
                var createdData = await _context.BiometricData
                    .Include(b => b.Staff)
                    .FirstOrDefaultAsync(b => b.Id == biometricData.Id);

                // Hide sensitive data when returning
                var result = new
                {
                    createdData!.Id,
                    createdData.StaffId,
                    Staff = new { createdData.Staff.Id, createdData.Staff.Name },
                    createdData.BiometricType,
                    HasTemplateData = true,
                    TemplateSize = createdData.TemplateData?.Length ?? 0,
                    createdData.IsActive,
                    createdData.CreatedAt,
                    createdData.UpdatedAt
                };

                _logger.LogInformation($"Created new biometric data: ID={biometricData.Id}, StaffID={biometricData.StaffId}, Type={biometricData.BiometricType}");
                return CreatedAtAction(nameof(GetBiometricData), new { id = biometricData.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating biometric data");
                return StatusCode(500, new { message = "Failed to create biometric data", error = ex.Message });
            }
        }

        /// <summary>
        /// Update biometric data
        /// </summary>
        /// <param name="id">Biometric data ID</param>
        /// <param name="biometricData">Updated biometric data information</param>
        /// <returns>Update result</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBiometricData(int id, BiometricData biometricData)
        {
            if (id != biometricData.Id)
            {
                return BadRequest(new { message = "ID in path does not match ID in request body" });
            }

            try
            {
                var existingData = await _context.BiometricData.FindAsync(id);
                if (existingData == null)
                {
                    return NotFound(new { message = $"Biometric data with ID {id} not found" });
                }

                // Validate biometric type
                var validTypes = new[] { "fingerprint", "face", "iris", "voice" };
                if (!validTypes.Contains(biometricData.BiometricType))
                {
                    return BadRequest(new { message = $"Invalid biometric type: {biometricData.BiometricType}" });
                }

                // If new template data is provided, encrypt it
                if (!string.IsNullOrWhiteSpace(biometricData.TemplateData))
                {
                    existingData.TemplateData = EncryptTemplateData(biometricData.TemplateData);
                }

                existingData.BiometricType = biometricData.BiometricType;
                existingData.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated biometric data: ID={id}");
                return Ok(new { message = "Biometric data updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating biometric data ID:{id}");
                return StatusCode(500, new { message = "Failed to update biometric data", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete biometric data (soft delete)
        /// </summary>
        /// <param name="id">Biometric data ID</param>
        /// <returns>Delete result</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBiometricData(int id)
        {
            try
            {
                var biometricData = await _context.BiometricData.FindAsync(id);
                if (biometricData == null)
                {
                    return NotFound(new { message = $"Biometric data with ID {id} not found" });
                }

                // Soft delete
                biometricData.IsActive = false;
                biometricData.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted biometric data: ID={id}");
                return Ok(new { message = "Biometric data deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting biometric data ID:{id}");
                return StatusCode(500, new { message = "Failed to delete biometric data", error = ex.Message });
            }
        }

        /// <summary>
        /// Biometric verification
        /// </summary>
        /// <param name="verifyRequest">Verification request</param>
        /// <returns>Verification result</returns>
        [HttpPost("verify")]
        public async Task<ActionResult> VerifyBiometric([FromBody] BiometricVerifyRequest verifyRequest)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(verifyRequest.TemplateData))
                {
                    return BadRequest(new { message = "Biometric template data cannot be empty" });
                }

                // Get all active biometric data of this type
                var biometricDataList = await _context.BiometricData
                    .Include(b => b.Staff)
                    .Where(b => b.BiometricType == verifyRequest.BiometricType && 
                               b.IsActive && 
                               b.Staff.IsActive)
                    .ToListAsync();

                if (!biometricDataList.Any())
                {
                    return NotFound(new { message = $"No biometric data found for {verifyRequest.BiometricType} type" });
                }

                // Simulate biometric matching algorithm
                var matchResult = await PerformBiometricMatching(verifyRequest.TemplateData, biometricDataList);

                if (matchResult.IsMatch)
                {
                    var staff = matchResult.MatchedStaff!;
                    staff.Role = staff.GetRoleFromId();

                    _logger.LogInformation($"Biometric verification successful: StaffID={staff.Id}, Type={verifyRequest.BiometricType}");
                    
                    return Ok(new
                    {
                        isMatch = true,
                        confidence = matchResult.Confidence,
                        staff = new
                        {
                            id = staff.Id,
                            name = staff.Name,
                            role = staff.Role,
                            email = staff.Email
                        },
                        message = "Biometric verification successful"
                    });
                }
                else
                {
                    _logger.LogWarning($"Biometric verification failed: Type={verifyRequest.BiometricType}");
                    return Ok(new
                    {
                        isMatch = false,
                        confidence = matchResult.Confidence,
                        message = "Biometric verification failed"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during biometric verification");
                return StatusCode(500, new { message = "Biometric verification failed", error = ex.Message });
            }
        }

        /// <summary>
        /// Get staff biometric data
        /// </summary>
        /// <param name="staffId">Staff ID</param>
        /// <returns>Staff biometric data list</returns>
        [HttpGet("staff/{staffId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetStaffBiometricData(int staffId)
        {
            try
            {
                var biometricData = await _context.BiometricData
                    .Include(b => b.Staff)
                    .Where(b => b.StaffId == staffId && b.IsActive)
                    .OrderBy(b => b.BiometricType)
                    .ToListAsync();

                if (!biometricData.Any())
                {
                    return NotFound(new { message = $"Staff {staffId} has no biometric data" });
                }

                // Hide sensitive data when returning
                var result = biometricData.Select(b => new
                {
                    b.Id,
                    b.BiometricType,
                    HasTemplateData = !string.IsNullOrEmpty(b.TemplateData),
                    TemplateSize = b.TemplateData?.Length ?? 0,
                    b.CreatedAt,
                    b.UpdatedAt
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting staff {staffId} biometric data");
                return StatusCode(500, new { message = "Failed to get staff biometric data", error = ex.Message });
            }
        }

        /// <summary>
        /// Encrypt template data
        /// </summary>
        /// <param name="templateData">Original template data</param>
        /// <returns>Encrypted data</returns>
        private string EncryptTemplateData(string templateData)
        {
            try
            {
                // Simple Base64 encoding (stronger encryption algorithms should be used in actual applications)
                var bytes = Encoding.UTF8.GetBytes(templateData);
                return Convert.ToBase64String(bytes);
            }
            catch
            {
                return templateData; // Return original data if encryption fails
            }
        }

        /// <summary>
        /// Decrypt template data
        /// </summary>
        /// <param name="encryptedData">Encrypted data</param>
        /// <returns>Decrypted data</returns>
        private string DecryptTemplateData(string encryptedData)
        {
            try
            {
                var bytes = Convert.FromBase64String(encryptedData);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return encryptedData; // Return original data if decryption fails
            }
        }

        /// <summary>
        /// Perform biometric matching
        /// </summary>
        /// <param name="inputTemplate">Input template data</param>
        /// <param name="storedTemplates">Stored template data list</param>
        /// <returns>Match result</returns>
        private async Task<BiometricMatchResult> PerformBiometricMatching(
            string inputTemplate, 
            List<BiometricData> storedTemplates)
        {
            await Task.Delay(100); // Simulate algorithm processing time

            // Simulate biometric matching algorithm
            foreach (var template in storedTemplates)
            {
                try
                {
                    var storedTemplate = DecryptTemplateData(template.TemplateData);
                    
                    // Simple string similarity matching (professional biometric matching algorithms should be used in actual applications)
                    var similarity = CalculateStringSimilarity(inputTemplate, storedTemplate);
                    
                    // Threshold set to 0.8 (80% similarity)
                    if (similarity >= 0.8)
                    {
                        return new BiometricMatchResult
                        {
                            IsMatch = true,
                            Confidence = similarity,
                            MatchedStaff = template.Staff
                        };
                    }
                }
                catch
                {
                    continue; // Skip error templates
                }
            }

            return new BiometricMatchResult
            {
                IsMatch = false,
                Confidence = 0.0,
                MatchedStaff = null
            };
        }

        /// <summary>
        /// Calculate string similarity
        /// </summary>
        /// <param name="str1">String 1</param>
        /// <param name="str2">String 2</param>
        /// <returns>Similarity (0-1)</returns>
        private double CalculateStringSimilarity(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0.0;

            if (str1 == str2)
                return 1.0;

            // Simple character matching algorithm
            var commonChars = str1.Intersect(str2).Count();
            var totalChars = Math.Max(str1.Length, str2.Length);
            
            return (double)commonChars / totalChars;
        }
    }

    /// <summary>
    /// Biometric verification request
    /// </summary>
    public class BiometricVerifyRequest
    {
        public string BiometricType { get; set; } = string.Empty;
        public string TemplateData { get; set; } = string.Empty;
    }

    /// <summary>
    /// Biometric match result
    /// </summary>
    public class BiometricMatchResult
    {
        public bool IsMatch { get; set; }
        public double Confidence { get; set; }
        public Staff? MatchedStaff { get; set; }
    }
}