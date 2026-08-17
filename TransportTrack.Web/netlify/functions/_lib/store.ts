import { getStore } from '@netlify/blobs';
import { getConnectionString, getDatabase } from '@netlify/database';

export interface DbDoc {
  id: number;
}

export interface DataStore {
  list<T extends DbDoc>(collection: string): Promise<T[]>;
  get<T extends DbDoc>(collection: string, id: number): Promise<T | null>;
  insert<T extends DbDoc>(collection: string, doc: Omit<T, 'id'>): Promise<T>;
  update<T extends DbDoc>(collection: string, id: number, doc: T): Promise<T | null>;
  remove(collection: string, id: number): Promise<void>;
}

type SqlTag = (strings: TemplateStringsArray, ...values: unknown[]) => Promise<Array<Record<string, unknown>>>;

const COLLECTION_KEY = (collection: string) => `c:${collection}`;
const SEQ_KEY = (collection: string) => `s:${collection}`;

class BlobStore implements DataStore {
  private readonly store = getStore({ name: 'transporttrack' });

  private async readArray<T>(collection: string): Promise<T[]> {
    const raw = (await this.store.get(COLLECTION_KEY(collection))) as unknown as string | null;
    return raw ? (JSON.parse(raw) as T[]) : [];
  }

  private async writeArray(collection: string, arr: unknown[]): Promise<void> {
    await this.store.setJSON(COLLECTION_KEY(collection), arr);
  }

  async list<T extends DbDoc>(collection: string): Promise<T[]> {
    return this.readArray<T>(collection);
  }

  async get<T extends DbDoc>(collection: string, id: number): Promise<T | null> {
    const arr = await this.readArray<T>(collection);
    return arr.find((doc) => doc.id === id) ?? null;
  }

  async insert<T extends DbDoc>(collection: string, doc: Omit<T, 'id'>): Promise<T> {
    const seqRaw = (await this.store.get(SEQ_KEY(collection))) as unknown as string | null;
    const next = (seqRaw ? Number.parseInt(seqRaw, 10) : 0) + 1;
    await this.store.set(SEQ_KEY(collection), String(next));
    const created = { ...doc, id: next } as T;
    const arr = await this.readArray<T>(collection);
    arr.push(created);
    await this.writeArray(collection, arr);
    return created;
  }

  async update<T extends DbDoc>(collection: string, id: number, doc: T): Promise<T | null> {
    const arr = await this.readArray<T>(collection);
    const index = arr.findIndex((d) => d.id === id);
    if (index === -1) {
      return null;
    }
    arr[index] = { ...doc, id };
    await this.writeArray(collection, arr);
    return arr[index];
  }

  async remove(collection: string, id: number): Promise<void> {
    const arr = await this.readArray<DbDoc>(collection);
    await this.writeArray(
      collection,
      arr.filter((d) => d.id !== id),
    );
  }
}

class PostgresStore implements DataStore {
  private initPromise: Promise<void> | null = null;
  private sql!: SqlTag;

  private async init(): Promise<void> {
    this.initPromise ??= (async () => {
      const { sql } = getDatabase();
      this.sql = sql as unknown as SqlTag;
      await this.sql`CREATE TABLE IF NOT EXISTS kv_docs (
        collection TEXT NOT NULL,
        id INTEGER NOT NULL,
        doc TEXT NOT NULL,
        PRIMARY KEY (collection, id)
      )`;
      await this.sql`CREATE TABLE IF NOT EXISTS kv_seqs (
        collection TEXT PRIMARY KEY,
        seq INTEGER NOT NULL
      )`;
    })();
    return this.initPromise;
  }

  async list<T extends DbDoc>(collection: string): Promise<T[]> {
    await this.init();
    const rows = await this.sql`SELECT doc FROM kv_docs WHERE collection = ${collection} ORDER BY id`;
    return rows.map((row) => JSON.parse(row.doc as string) as T);
  }

  async get<T extends DbDoc>(collection: string, id: number): Promise<T | null> {
    await this.init();
    const rows = await this.sql`SELECT doc FROM kv_docs WHERE collection = ${collection} AND id = ${id}`;
    return rows.length > 0 ? (JSON.parse(rows[0].doc as string) as T) : null;
  }

  async insert<T extends DbDoc>(collection: string, doc: Omit<T, 'id'>): Promise<T> {
    await this.init();
    const rows = await this.sql`INSERT INTO kv_seqs (collection, seq)
      VALUES (${collection}, 1)
      ON CONFLICT (collection) DO UPDATE SET seq = kv_seqs.seq + 1
      RETURNING seq`;
    const next = rows[0].seq as number;
    const created = { ...doc, id: next } as T;
    await this.sql`INSERT INTO kv_docs (collection, id, doc) VALUES (${collection}, ${next}, ${JSON.stringify(created)})`;
    return created;
  }

  async update<T extends DbDoc>(collection: string, id: number, doc: T): Promise<T | null> {
    await this.init();
    const rows = await this.sql`UPDATE kv_docs SET doc = ${JSON.stringify({ ...doc, id })}
      WHERE collection = ${collection} AND id = ${id}
      RETURNING doc`;
    return rows.length > 0 ? (JSON.parse(rows[0].doc as string) as T) : null;
  }

  async remove(collection: string, id: number): Promise<void> {
    await this.init();
    await this.sql`DELETE FROM kv_docs WHERE collection = ${collection} AND id = ${id}`;
  }
}

let storePromise: Promise<DataStore> | null = null;

export function getDataStore(): Promise<DataStore> {
  storePromise ??= (async () => {
    try {
      getConnectionString();
      return new PostgresStore();
    } catch {
      return new BlobStore();
    }
  })();
  return storePromise;
}
