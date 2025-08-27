import { eventApi } from '../api/client'
import { OfflineEvent } from '../types/api'
import { 
  addOfflineEvent,
  getOfflineEvents,
  removeOfflineEvent,
  getOfflineEventCount
} from './db'

let isSyncing = false
let lastSyncTime: Date | null = null

export interface SyncStatus {
  isOnline: boolean
  queueLength: number
  lastSync: Date | null
  isSyncing: boolean
}

// Enqueue event for offline storage
export async function enqueueEvent(eventDraft: Omit<OfflineEvent, 'id' | 'timestamp'>): Promise<void> {
  try {
    // First try to send to server if online
    if (navigator.onLine) {
      await eventApi.create(eventDraft)
      console.log('Event sent successfully to server')
      return
    }
  } catch (error) {
    console.warn('Failed to send event to server, queuing for later:', error)
  }

  // If offline or server error, add to queue
  await addOfflineEvent(eventDraft)
  console.log('Event queued for offline sync')
}

// Flush offline queue
export async function flushQueue(): Promise<{ success: number; failed: number }> {
  if (isSyncing) {
    console.warn('Sync already in progress')
    return { success: 0, failed: 0 }
  }

  if (!navigator.onLine) {
    console.warn('Cannot sync - device is offline')
    return { success: 0, failed: 0 }
  }

  isSyncing = true
  let successCount = 0
  let failedCount = 0

  try {
    const events = await getOfflineEvents()
    console.log(`Starting sync of ${events.length} offline events`)

    for (const event of events) {
      try {
        // Remove local properties before sending to server
        const { id, timestamp, ...eventData } = event
        await eventApi.create(eventData)
        
        // Remove from queue on success
        if (event.id) {
          await removeOfflineEvent(event.id)
        }
        successCount++
        console.log(`Successfully synced event ${event.id}`)
      } catch (error) {
        console.error(`Failed to sync event ${event.id}:`, error)
        failedCount++
        // Stop on first failure to maintain order
        break
      }
    }

    if (successCount > 0) {
      lastSyncTime = new Date()
    }

    console.log(`Sync completed: ${successCount} success, ${failedCount} failed`)
  } catch (error) {
    console.error('Queue flush failed:', error)
  } finally {
    isSyncing = false
  }

  return { success: successCount, failed: failedCount }
}

// Get current sync status
export async function getSyncStatus(): Promise<SyncStatus> {
  const queueLength = await getOfflineEventCount()
  
  return {
    isOnline: navigator.onLine,
    queueLength,
    lastSync: lastSyncTime,
    isSyncing
  }
}

// Auto sync when coming back online
let onlineHandler: (() => void) | null = null

export function startAutoSync(): void {
  // Remove existing handler if any
  if (onlineHandler) {
    window.removeEventListener('online', onlineHandler)
  }

  onlineHandler = async () => {
    console.log('Network connection restored, attempting to sync...')
    await new Promise(resolve => setTimeout(resolve, 1000)) // Wait 1 second for connection to stabilize
    await flushQueue()
  }

  window.addEventListener('online', onlineHandler)
}

export function stopAutoSync(): void {
  if (onlineHandler) {
    window.removeEventListener('online', onlineHandler)
    onlineHandler = null
  }
} 