import { openDB, DBSchema, IDBPDatabase } from 'idb'
import { OfflineEvent } from '../types/api'

interface AssignmentDB extends DBSchema {
  offline_events: {
    key: string
    value: OfflineEvent
    indexes: { 'by-timestamp': number; 'by-staffId': number }
  }
}

let db: IDBPDatabase<AssignmentDB> | null = null

export async function initDB(): Promise<IDBPDatabase<AssignmentDB>> {
  if (db) return db

  db = await openDB<AssignmentDB>('assignment-db', 1, {
    upgrade(db) {
      // Create offline events store
      const eventsStore = db.createObjectStore('offline_events', {
        keyPath: 'id'
      })
      eventsStore.createIndex('by-timestamp', 'timestamp')
      eventsStore.createIndex('by-staffId', 'staffId')
    },
  })

  return db
}

export async function getDB(): Promise<IDBPDatabase<AssignmentDB>> {
  if (!db) {
    db = await initDB()
  }
  return db
}

// Offline events operations
export async function addOfflineEvent(event: OfflineEvent): Promise<void> {
  const database = await getDB()
  const eventWithId = {
    ...event,
    id: `offline_${Date.now()}_${Math.random()}`,
    timestamp: Date.now()
  }
  await database.add('offline_events', eventWithId)
}

export async function getOfflineEvents(): Promise<OfflineEvent[]> {
  const database = await getDB()
  return database.getAllFromIndex('offline_events', 'by-timestamp')
}

export async function removeOfflineEvent(id: string): Promise<void> {
  const database = await getDB()
  await database.delete('offline_events', id)
}

export async function clearOfflineEvents(): Promise<void> {
  const database = await getDB()
  const tx = database.transaction(['offline_events'], 'readwrite')
  await tx.objectStore('offline_events').clear()
  await tx.done
}

export async function getOfflineEventCount(): Promise<number> {
  const database = await getDB()
  return database.count('offline_events')
} 