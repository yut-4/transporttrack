import type { TrackingPingRequest } from './types';

const DB_NAME = 'transporttrack';
const DB_VERSION = 1;
const STORE = 'location_outbox';

function openDb(): Promise<IDBDatabase> {
  return new Promise((resolve, reject) => {
    const req = indexedDB.open(DB_NAME, DB_VERSION);
    req.onupgradeneeded = () => {
      const db = req.result;
      if (!db.objectStoreNames.contains(STORE)) {
        const store = db.createObjectStore(STORE, { keyPath: 'clientPingId' });
        store.createIndex('timestamp', 'recordedAtUtc');
      }
    };
    req.onsuccess = () => resolve(req.result);
    req.onerror = () => reject(req.error ?? new Error('No se pudo abrir IndexedDB'));
  });
}

function withStore<T>(
  mode: IDBTransactionMode,
  fn: (store: IDBObjectStore) => IDBRequest<T>,
): Promise<T> {
  return openDb().then(
    (db) =>
      new Promise<T>((resolve, reject) => {
        const tx = db.transaction(STORE, mode);
        const store = tx.objectStore(STORE);
        const req = fn(store);
        req.onsuccess = () => resolve(req.result);
        req.onerror = () => reject(req.error ?? new Error('Error en IndexedDB'));
        tx.oncomplete = () => db.close();
        tx.onerror = () => reject(tx.error ?? new Error('Error en transacción IndexedDB'));
      }),
  );
}

export function dbSupported(): boolean {
  return typeof indexedDB !== 'undefined';
}

export async function enqueuePings(pings: TrackingPingRequest[]): Promise<void> {
  if (!dbSupported() || pings.length === 0) return;
  await withStore('readwrite', (store) => {
    const putReq = store.put(pings[0]);
    pings.slice(1).forEach((p) => store.put(p));
    return putReq;
  });
}

export async function peekPings(max: number): Promise<TrackingPingRequest[]> {
  if (!dbSupported()) return [];
  const all = await withStore<TrackingPingRequest[]>('readonly', (store) => {
    const req = store.getAll() as IDBRequest<TrackingPingRequest[]>;
    return req;
  });
  return all.sort((a, b) => a.recordedAtUtc.localeCompare(b.recordedAtUtc)).slice(0, max);
}

export async function removePings(clientPingIds: string[]): Promise<void> {
  if (!dbSupported() || clientPingIds.length === 0) return;
  await withStore('readwrite', (store) => {
    const delReq = store.delete(clientPingIds[0]);
    clientPingIds.slice(1).forEach((id) => store.delete(id));
    return delReq;
  });
}

export async function countOutbox(): Promise<number> {
  if (!dbSupported()) return 0;
  return withStore('readonly', (store) => store.count() as IDBRequest<number>);
}