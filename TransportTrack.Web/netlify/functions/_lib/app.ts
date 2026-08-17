import type { DataStore, DbDoc } from './store';

export class ApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

export interface ConductorDoc extends DbDoc {
  nombre: string;
  licencia: string;
  telefono: string;
}

export interface VehiculoDoc extends DbDoc {
  placa: string;
  marca: string;
  modelo: string;
  anio: number;
  conductorId: number;
  nombreConductor: string | null;
}

export interface RutaDoc extends DbDoc {
  origen: string;
  destino: string;
  conductorId: number;
  nombreConductor: string | null;
  vehiculoId: number;
  placaVehiculo: string | null;
  fechaSalida: string;
  fechaLlegada: string | null;
}

export interface DeviceDoc extends DbDoc {
  installationId: string;
  conductorId: number;
  platform: string;
  name: string | null;
  isActive: boolean;
  createdAtUtc: string;
  lastSeenAtUtc: string;
}

export interface SessionDoc extends DbDoc {
  conductorId: number;
  conductorNombre: string;
  vehiculoId: number | null;
  placa: string | null;
  rutaId: number | null;
  origen: string | null;
  destino: string | null;
  status: 'active' | 'completed';
  startedAtUtc: string;
  deviceName: string | null;
  completedAtUtc: string | null;
}

export interface PingDoc extends DbDoc {
  clientPingId: string;
  sessionId: number;
  conductorId: number;
  vehiculoId: number | null;
  rutaId: number | null;
  latitude: number;
  longitude: number;
  accuracyMeters: number | null;
  altitudeMeters: number | null;
  speedMetersPerSecond: number | null;
  headingDegrees: number | null;
  recordedAtUtc: string;
  receivedAtUtc: string;
  source: string;
}

export interface TrackingSessionInfo {
  sessionId: number;
  conductorId: number;
  conductorNombre: string;
  vehiculoId: number | null;
  placa: string | null;
  rutaId: number | null;
  origen: string | null;
  destino: string | null;
  status: 'active' | 'completed' | 'cancelled';
  startedAtUtc: string;
  deviceName: string | null;
}

export interface DriverLocation {
  conductorId: number;
  conductorNombre: string;
  vehiculoId: number | null;
  placa: string | null;
  rutaId: number | null;
  origen: string | null;
  destino: string | null;
  latitude: number;
  longitude: number;
  accuracyMeters: number | null;
  speedMetersPerSecond: number | null;
  headingDegrees: number | null;
  recordedAtUtc: string;
  receivedAtUtc: string;
  state: 'moving' | 'stopped' | 'stale' | 'offline';
}

const LIVE_WINDOW_MS = 300_000;
const LIVE_THRESHOLD_MS = 90_000;
const MOVING_SPEED_THRESHOLD = 2.0;

const COLLECTIONS = {
  conductores: 'conductores',
  vehiculos: 'vehiculos',
  rutas: 'rutas',
  devices: 'tracking_devices',
  sessions: 'tracking_sessions',
  pings: 'location_pings',
} as const;

function nowIso(): string {
  return new Date().toISOString();
}

function firstDefined<T>(value: T | null | undefined, fallback: T): T {
  return value === null || value === undefined ? fallback : value;
}

export class Api {
  constructor(private readonly store: DataStore) {}

  // ==================== Entidades ====================

  async ensureSeeded(): Promise<void> {
    const existing = await this.store.list<ConductorDoc>(COLLECTIONS.conductores);
    if (existing.length > 0) {
      return;
    }

    const ana = await this.store.insert<ConductorDoc>(COLLECTIONS.conductores, {
      nombre: 'Ana Martínez',
      licencia: 'LIC-001',
      telefono: '+34 611 22 33 44',
    });
    const carlos = await this.store.insert<ConductorDoc>(COLLECTIONS.conductores, {
      nombre: 'Carlos Gómez',
      licencia: 'LIC-002',
      telefono: '+34 622 55 66 77',
    });

    const camioneta = await this.store.insert<VehiculoDoc>(COLLECTIONS.vehiculos, {
      placa: 'ABC-1234',
      marca: 'Mercedes',
      modelo: 'Sprinter',
      anio: 2022,
      conductorId: ana.id,
      nombreConductor: ana.nombre,
    });
    const furgoneta = await this.store.insert<VehiculoDoc>(COLLECTIONS.vehiculos, {
      placa: 'XYZ-9876',
      marca: 'Ford',
      modelo: 'Transit',
      anio: 2021,
      conductorId: carlos.id,
      nombreConductor: carlos.nombre,
    });

    await this.store.insert<RutaDoc>(COLLECTIONS.rutas, {
      origen: 'Madrid',
      destino: 'Barcelona',
      conductorId: ana.id,
      nombreConductor: ana.nombre,
      vehiculoId: camioneta.id,
      placaVehiculo: camioneta.placa,
      fechaSalida: nowIso(),
      fechaLlegada: null,
    });
    await this.store.insert<RutaDoc>(COLLECTIONS.rutas, {
      origen: 'Valencia',
      destino: 'Sevilla',
      conductorId: carlos.id,
      nombreConductor: carlos.nombre,
      vehiculoId: furgoneta.id,
      placaVehiculo: furgoneta.placa,
      fechaSalida: nowIso(),
      fechaLlegada: null,
    });
  }

  async listConductores(): Promise<ConductorDoc[]> {
    await this.ensureSeeded();
    return this.store.list<ConductorDoc>(COLLECTIONS.conductores);
  }

  async getConductor(id: number): Promise<ConductorDoc | null> {
    return this.store.get<ConductorDoc>(COLLECTIONS.conductores, id);
  }

  async createConductor(body: { nombre?: string; licencia?: string; telefono?: string }): Promise<ConductorDoc> {
    const nombre = (body.nombre ?? '').trim();
    const licencia = (body.licencia ?? '').trim();
    const telefono = (body.telefono ?? '').trim();
    if (!nombre || !licencia || !telefono) {
      throw new ApiError(400, 'Nombre, licencia y teléfono son obligatorios.');
    }

    const all = await this.store.list<ConductorDoc>(COLLECTIONS.conductores);
    if (all.some((c) => c.licencia === licencia)) {
      throw new ApiError(400, 'Ya existe un conductor con la licencia indicada.');
    }

    return this.store.insert<ConductorDoc>(COLLECTIONS.conductores, { nombre, licencia, telefono });
  }

  async updateConductor(id: number, body: { nombre?: string; licencia?: string; telefono?: string }): Promise<ConductorDoc | null> {
    const existing = await this.store.get<ConductorDoc>(COLLECTIONS.conductores, id);
    if (!existing) {
      throw new ApiError(404, 'El conductor indicado no existe.');
    }

    const nombre = (body.nombre ?? existing.nombre).trim();
    const licencia = (body.licencia ?? existing.licencia).trim();
    const telefono = (body.telefono ?? existing.telefono).trim();
    const all = await this.store.list<ConductorDoc>(COLLECTIONS.conductores);
    if (all.some((c) => c.licencia === licencia && c.id !== id)) {
      throw new ApiError(400, 'Ya existe otro conductor con la licencia indicada.');
    }

    return this.store.update<ConductorDoc>(COLLECTIONS.conductores, id, { ...existing, nombre, licencia, telefono });
  }

  async deleteConductor(id: number): Promise<void> {
    const existing = await this.store.get<ConductorDoc>(COLLECTIONS.conductores, id);
    if (!existing) {
      throw new ApiError(400, 'El conductor indicado no existe.');
    }
    await this.store.remove(COLLECTIONS.conductores, id);
  }

  async listVehiculos(): Promise<VehiculoDoc[]> {
    await this.ensureSeeded();
    return this.store.list<VehiculoDoc>(COLLECTIONS.vehiculos);
  }

  async getVehiculo(id: number): Promise<VehiculoDoc | null> {
    return this.store.get<VehiculoDoc>(COLLECTIONS.vehiculos, id);
  }

  async createVehiculo(body: { placa?: string; marca?: string; modelo?: string; anio?: number; conductorId?: number }): Promise<VehiculoDoc> {
    const placa = (body.placa ?? '').trim();
    const marca = (body.marca ?? '').trim();
    const modelo = (body.modelo ?? '').trim();
    const anio = body.anio;
    const conductorId = body.conductorId;
    if (!placa || !marca || !modelo || !anio || !conductorId) {
      throw new ApiError(400, 'Placa, marca, modelo, año y conductor son obligatorios.');
    }

    const all = await this.store.list<VehiculoDoc>(COLLECTIONS.vehiculos);
    if (all.some((v) => v.placa === placa)) {
      throw new ApiError(400, 'Ya existe un vehículo con la placa indicada.');
    }

    const conductor = await this.store.get<ConductorDoc>(COLLECTIONS.conductores, conductorId);
    if (!conductor) {
      throw new ApiError(400, 'El conductor indicado no existe.');
    }

    return this.store.insert<VehiculoDoc>(COLLECTIONS.vehiculos, {
      placa,
      marca,
      modelo,
      anio,
      conductorId,
      nombreConductor: conductor.nombre,
    });
  }

  async updateVehiculo(id: number, body: { placa?: string; marca?: string; modelo?: string; anio?: number; conductorId?: number }): Promise<VehiculoDoc | null> {
    const existing = await this.store.get<VehiculoDoc>(COLLECTIONS.vehiculos, id);
    if (!existing) {
      throw new ApiError(404, 'El vehículo indicado no existe.');
    }

    const placa = (body.placa ?? existing.placa).trim();
    const conductorId = body.conductorId ?? existing.conductorId;
    const all = await this.store.list<VehiculoDoc>(COLLECTIONS.vehiculos);
    if (all.some((v) => v.placa === placa && v.id !== id)) {
      throw new ApiError(400, 'Ya existe otro vehículo con la placa indicada.');
    }

    const conductor = await this.store.get<ConductorDoc>(COLLECTIONS.conductores, conductorId);
    if (!conductor) {
      throw new ApiError(400, 'El conductor indicado no existe.');
    }

    return this.store.update<VehiculoDoc>(COLLECTIONS.vehiculos, id, {
      ...existing,
      placa,
      marca: (body.marca ?? existing.marca).trim(),
      modelo: (body.modelo ?? existing.modelo).trim(),
      anio: body.anio ?? existing.anio,
      conductorId,
      nombreConductor: conductor.nombre,
    });
  }

  async deleteVehiculo(id: number): Promise<void> {
    const existing = await this.store.get<VehiculoDoc>(COLLECTIONS.vehiculos, id);
    if (!existing) {
      throw new ApiError(404, 'El vehículo indicado no existe.');
    }
    await this.store.remove(COLLECTIONS.vehiculos, id);
  }

  async listRutas(): Promise<RutaDoc[]> {
    await this.ensureSeeded();
    return this.store.list<RutaDoc>(COLLECTIONS.rutas);
  }

  async getRuta(id: number): Promise<RutaDoc | null> {
    return this.store.get<RutaDoc>(COLLECTIONS.rutas, id);
  }

  async createRuta(body: {
    origen?: string;
    destino?: string;
    conductorId?: number;
    vehiculoId?: number;
    fechaSalida?: string | null;
    fechaLlegada?: string | null;
  }): Promise<RutaDoc> {
    const origen = (body.origen ?? '').trim();
    const destino = (body.destino ?? '').trim();
    const conductorId = body.conductorId;
    const vehiculoId = body.vehiculoId;
    if (!origen || !destino || !conductorId || !vehiculoId) {
      throw new ApiError(400, 'Origen, destino, conductor y vehículo son obligatorios.');
    }

    const conductor = await this.store.get<ConductorDoc>(COLLECTIONS.conductores, conductorId);
    if (!conductor) {
      throw new ApiError(400, 'El conductor indicado no existe.');
    }
    const vehiculo = await this.store.get<VehiculoDoc>(COLLECTIONS.vehiculos, vehiculoId);
    if (!vehiculo) {
      throw new ApiError(400, 'El vehículo indicado no existe.');
    }

    return this.store.insert<RutaDoc>(COLLECTIONS.rutas, {
      origen,
      destino,
      conductorId,
      nombreConductor: conductor.nombre,
      vehiculoId,
      placaVehiculo: vehiculo.placa,
      fechaSalida: body.fechaSalida ?? nowIso(),
      fechaLlegada: body.fechaLlegada ?? null,
    });
  }

  async updateRuta(id: number, body: {
    origen?: string;
    destino?: string;
    conductorId?: number;
    vehiculoId?: number;
    fechaSalida?: string | null;
    fechaLlegada?: string | null;
  }): Promise<RutaDoc | null> {
    const existing = await this.store.get<RutaDoc>(COLLECTIONS.rutas, id);
    if (!existing) {
      throw new ApiError(404, 'La ruta indicada no existe.');
    }

    const conductorId = body.conductorId ?? existing.conductorId;
    const vehiculoId = body.vehiculoId ?? existing.vehiculoId;
    const conductor = await this.store.get<ConductorDoc>(COLLECTIONS.conductores, conductorId);
    if (!conductor) {
      throw new ApiError(400, 'El conductor indicado no existe.');
    }
    const vehiculo = await this.store.get<VehiculoDoc>(COLLECTIONS.vehiculos, vehiculoId);
    if (!vehiculo) {
      throw new ApiError(400, 'El vehículo indicado no existe.');
    }

    return this.store.update<RutaDoc>(COLLECTIONS.rutas, id, {
      ...existing,
      origen: (body.origen ?? existing.origen).trim(),
      destino: (body.destino ?? existing.destino).trim(),
      conductorId,
      nombreConductor: conductor.nombre,
      vehiculoId,
      placaVehiculo: vehiculo.placa,
      fechaSalida: body.fechaSalida ?? existing.fechaSalida,
      fechaLlegada: firstDefined(body.fechaLlegada, existing.fechaLlegada),
    });
  }

  async deleteRuta(id: number): Promise<void> {
    const existing = await this.store.get<RutaDoc>(COLLECTIONS.rutas, id);
    if (!existing) {
      throw new ApiError(404, 'La ruta indicada no existe.');
    }
    await this.store.remove(COLLECTIONS.rutas, id);
  }

  // ==================== Tracking ====================

  async pairDevice(body: {
    installationId?: string;
    platform?: string;
    name?: string | null;
    conductorId?: number;
  }): Promise<{ deviceId: number; conductorId: number; conductorNombre: string }> {
    const installationId = (body.installationId ?? '').trim();
    if (!installationId || installationId.length > 64) {
      throw new ApiError(400, 'El identificador de instalación es obligatorio.');
    }
    if (!body.conductorId) {
      throw new ApiError(400, 'El conductor es obligatorio.');
    }

    const conductor = await this.store.get<ConductorDoc>(COLLECTIONS.conductores, body.conductorId);
    if (!conductor) {
      throw new ApiError(404, 'El conductor indicado no existe.');
    }

    const devices = await this.store.list<DeviceDoc>(COLLECTIONS.devices);
    const existing = devices.find((d) => d.installationId === installationId);
    let device: DeviceDoc;
    if (existing) {
      device = (await this.store.update<DeviceDoc>(COLLECTIONS.devices, existing.id, {
        ...existing,
        conductorId: conductor.id,
        name: body.name ?? existing.name,
        platform: body.platform ?? existing.platform,
        isActive: true,
        lastSeenAtUtc: nowIso(),
      }))!;
    } else {
      device = await this.store.insert<DeviceDoc>(COLLECTIONS.devices, {
        installationId,
        conductorId: conductor.id,
        platform: body.platform || 'web',
        name: body.name || null,
        isActive: true,
        createdAtUtc: nowIso(),
        lastSeenAtUtc: nowIso(),
      });
    }

    return { deviceId: device.id, conductorId: conductor.id, conductorNombre: conductor.nombre };
  }

  private async resolveDevice(installationId: string | null | undefined): Promise<DeviceDoc> {
    if (!installationId || !installationId.trim()) {
      throw new ApiError(401, 'Dispositivo no identificado.');
    }
    const devices = await this.store.list<DeviceDoc>(COLLECTIONS.devices);
    const device = devices.find((d) => d.installationId === installationId.trim() && d.isActive);
    if (!device) {
      throw new ApiError(401, 'Dispositivo no vinculado. Ejecute el emparejamiento primero.');
    }
    return device;
  }

  private toSessionInfo(session: SessionDoc): TrackingSessionInfo {
    return {
      sessionId: session.id,
      conductorId: session.conductorId,
      conductorNombre: session.conductorNombre,
      vehiculoId: session.vehiculoId,
      placa: session.placa,
      rutaId: session.rutaId,
      origen: session.origen,
      destino: session.destino,
      status: session.status,
      startedAtUtc: session.startedAtUtc,
      deviceName: session.deviceName,
    };
  }

  async startSession(
    body: { conductorId?: number; vehiculoId?: number | null; rutaId?: number | null },
    installationId: string | null | undefined,
  ): Promise<TrackingSessionInfo> {
    const device = await this.resolveDevice(installationId);

    if (body.conductorId !== device.conductorId) {
      throw new ApiError(403, 'La sesión debe corresponder al conductor vinculado al dispositivo.');
    }

    if (body.vehiculoId != null) {
      const vehiculo = await this.store.get<VehiculoDoc>(COLLECTIONS.vehiculos, body.vehiculoId);
      if (!vehiculo) {
        throw new ApiError(400, 'El vehículo indicado no existe.');
      }
      if (vehiculo.conductorId !== device.conductorId) {
        throw new ApiError(400, 'El vehículo no corresponde al conductor.');
      }
    }

    if (body.rutaId != null) {
      const ruta = await this.store.get<RutaDoc>(COLLECTIONS.rutas, body.rutaId);
      if (!ruta) {
        throw new ApiError(400, 'La ruta indicada no existe.');
      }
    }

    const sessions = await this.store.list<SessionDoc>(COLLECTIONS.sessions);
    if (sessions.some((s) => s.conductorId === device.conductorId && s.status === 'active')) {
      throw new ApiError(400, 'El conductor ya tiene una sesión activa.');
    }

    const conductor = await this.store.get<ConductorDoc>(COLLECTIONS.conductores, device.conductorId);
    const vehiculo = body.vehiculoId != null ? await this.store.get<VehiculoDoc>(COLLECTIONS.vehiculos, body.vehiculoId) : null;
    const ruta = body.rutaId != null ? await this.store.get<RutaDoc>(COLLECTIONS.rutas, body.rutaId) : null;

    const session = await this.store.insert<SessionDoc>(COLLECTIONS.sessions, {
      conductorId: device.conductorId,
      conductorNombre: conductor?.nombre ?? '',
      vehiculoId: body.vehiculoId ?? null,
      placa: vehiculo?.placa ?? null,
      rutaId: body.rutaId ?? null,
      origen: ruta?.origen ?? null,
      destino: ruta?.destino ?? null,
      status: 'active',
      startedAtUtc: nowIso(),
      deviceName: device.name,
      completedAtUtc: null,
    });

    await this.store.update<DeviceDoc>(COLLECTIONS.devices, device.id, { ...device, lastSeenAtUtc: nowIso() });

    return this.toSessionInfo(session);
  }

  async stopSession(sessionId: number, conductorId: number): Promise<TrackingSessionInfo> {
    const sessions = await this.store.list<SessionDoc>(COLLECTIONS.sessions);
    const session = sessions.find((s) => s.id === sessionId && s.conductorId === conductorId);
    if (!session) {
      throw new ApiError(404, 'La sesión indicada no existe.');
    }

    const updated = session.status === 'active'
      ? await this.store.update<SessionDoc>(COLLECTIONS.sessions, sessionId, {
          ...session,
          status: 'completed',
          completedAtUtc: nowIso(),
        })
      : session;

    return this.toSessionInfo(updated ?? session);
  }

  async getActiveSession(conductorId: number): Promise<TrackingSessionInfo | null> {
    const sessions = await this.store.list<SessionDoc>(COLLECTIONS.sessions);
    const session = sessions.find((s) => s.conductorId === conductorId && s.status === 'active');
    return session ? this.toSessionInfo(session) : null;
  }

  private validatePing(ping: {
    clientPingId?: string;
    latitude?: number;
    longitude?: number;
    accuracyMeters?: number | null;
    speedMetersPerSecond?: number | null;
    headingDegrees?: number | null;
  }): string | null {
    if (!ping.clientPingId || ping.clientPingId.length > 36) {
      return 'El identificador de ping es obligatorio.';
    }
    if (ping.latitude === undefined || ping.latitude < -90 || ping.latitude > 90) {
      return 'La latitud está fuera de rango.';
    }
    if (ping.longitude === undefined || ping.longitude < -180 || ping.longitude > 180) {
      return 'La longitud está fuera de rango.';
    }
    if (ping.accuracyMeters != null && ping.accuracyMeters < 0) {
      return 'La precisión no puede ser negativa.';
    }
    if (ping.speedMetersPerSecond != null && ping.speedMetersPerSecond < 0) {
      return 'La velocidad no puede ser negativa.';
    }
    if (ping.headingDegrees != null && (ping.headingDegrees < 0 || ping.headingDegrees > 360)) {
      return 'El rumbo debe estar entre 0 y 360 grados.';
    }
    return null;
  }

  private buildPing(ping: {
    clientPingId: string;
    sessionId: number;
    latitude: number;
    longitude: number;
    accuracyMeters?: number | null;
    altitudeMeters?: number | null;
    speedMetersPerSecond?: number | null;
    headingDegrees?: number | null;
    recordedAtUtc?: string;
    source?: string;
  }, session: SessionDoc): Omit<PingDoc, 'id'> {
    const recordedAt = ping.recordedAtUtc ? new Date(ping.recordedAtUtc).toISOString() : nowIso();
    const capped = new Date(recordedAt).getTime() > Date.now() + 5 * 60_000 ? nowIso() : recordedAt;

    return {
      clientPingId: ping.clientPingId,
      sessionId: session.id,
      conductorId: session.conductorId,
      vehiculoId: session.vehiculoId,
      rutaId: session.rutaId,
      latitude: ping.latitude,
      longitude: ping.longitude,
      accuracyMeters: ping.accuracyMeters ?? null,
      altitudeMeters: ping.altitudeMeters ?? null,
      speedMetersPerSecond: ping.speedMetersPerSecond ?? null,
      headingDegrees: ping.headingDegrees ?? null,
      recordedAtUtc: capped,
      receivedAtUtc: nowIso(),
      source: ping.source || 'web',
    };
  }

  async addPing(ping: {
    clientPingId?: string;
    sessionId?: number;
    latitude?: number;
    longitude?: number;
    accuracyMeters?: number | null;
    altitudeMeters?: number | null;
    speedMetersPerSecond?: number | null;
    headingDegrees?: number | null;
    recordedAtUtc?: string;
    source?: string;
  }, installationId: string | null | undefined): Promise<{ accepted: boolean }> {
    const device = await this.resolveDevice(installationId);

    const error = this.validatePing(ping);
    if (error) {
      throw new ApiError(400, error);
    }

    const sessions = await this.store.list<SessionDoc>(COLLECTIONS.sessions);
    const session = sessions.find((s) => s.id === ping.sessionId);
    if (!session) {
      throw new ApiError(404, 'La sesión indicada no existe.');
    }
    if (session.conductorId !== device.conductorId) {
      throw new ApiError(403, 'La sesión no corresponde al dispositivo.');
    }
    if (session.status !== 'active') {
      throw new ApiError(400, 'La sesión no está activa.');
    }

    const all = await this.store.list<PingDoc>(COLLECTIONS.pings);
    if (all.some((p) => p.clientPingId === ping.clientPingId)) {
      return { accepted: true };
    }

    await this.store.insert<PingDoc>(COLLECTIONS.pings, this.buildPing(ping as Parameters<typeof this.buildPing>[0], session));
    await this.store.update<DeviceDoc>(COLLECTIONS.devices, device.id, { ...device, lastSeenAtUtc: nowIso() });

    return { accepted: true };
  }

  async addPingsBatch(body: {
    pings?: Array<{
      clientPingId?: string;
      sessionId?: number;
      latitude?: number;
      longitude?: number;
      accuracyMeters?: number | null;
      altitudeMeters?: number | null;
      speedMetersPerSecond?: number | null;
      headingDegrees?: number | null;
      recordedAtUtc?: string;
      source?: string;
    }>;
  }, installationId: string | null | undefined): Promise<{ accepted: number }> {
    const device = await this.resolveDevice(installationId);

    const all = await this.store.list<PingDoc>(COLLECTIONS.pings);
    const sessions = await this.store.list<SessionDoc>(COLLECTIONS.sessions);
    let accepted = 0;
    const pending: Array<Omit<PingDoc, 'id'>> = [];

    for (const ping of body.pings ?? []) {
      if (this.validatePing(ping)) {
        continue;
      }
      if (all.some((p) => p.clientPingId === ping.clientPingId)) {
        accepted++;
        continue;
      }
      const session = sessions.find((s) => s.id === ping.sessionId);
      if (!session || session.conductorId !== device.conductorId || session.status !== 'active') {
        continue;
      }
      pending.push(this.buildPing(ping as Parameters<typeof this.buildPing>[0], session));
      accepted++;
    }

    for (const ping of pending) {
      await this.store.insert<PingDoc>(COLLECTIONS.pings, ping);
    }
    await this.store.update<DeviceDoc>(COLLECTIONS.devices, device.id, { ...device, lastSeenAtUtc: nowIso() });

    return { accepted };
  }

  private computeState(recordedAtUtc: string, speed: number | null): DriverLocation['state'] {
    const age = Date.now() - new Date(recordedAtUtc).getTime();
    if (age <= LIVE_THRESHOLD_MS) {
      return speed != null && speed >= MOVING_SPEED_THRESHOLD ? 'moving' : 'stopped';
    }
    if (age <= LIVE_WINDOW_MS) {
      return 'stale';
    }
    return 'offline';
  }

  private toDriverLocation(
    ping: PingDoc,
    conductor: ConductorDoc | null,
    vehiculo: VehiculoDoc | null,
    ruta: RutaDoc | null,
  ): DriverLocation {
    return {
      conductorId: ping.conductorId,
      conductorNombre: conductor?.nombre ?? '',
      vehiculoId: ping.vehiculoId,
      placa: vehiculo?.placa ?? null,
      rutaId: ping.rutaId,
      origen: ruta?.origen ?? null,
      destino: ruta?.destino ?? null,
      latitude: ping.latitude,
      longitude: ping.longitude,
      accuracyMeters: ping.accuracyMeters,
      speedMetersPerSecond: ping.speedMetersPerSecond,
      headingDegrees: ping.headingDegrees,
      recordedAtUtc: ping.recordedAtUtc,
      receivedAtUtc: ping.receivedAtUtc,
      state: this.computeState(ping.recordedAtUtc, ping.speedMetersPerSecond),
    };
  }

  async getLive(): Promise<DriverLocation[]> {
    const cutoff = new Date(Date.now() - LIVE_WINDOW_MS);
    const pings = await this.store.list<PingDoc>(COLLECTIONS.pings);
    const recent = pings.filter((p) => new Date(p.recordedAtUtc).getTime() >= cutoff.getTime());
    if (recent.length === 0) {
      return [];
    }

    const latestByConductor = new Map<number, PingDoc>();
    for (const ping of [...recent].sort((a, b) => b.recordedAtUtc.localeCompare(a.recordedAtUtc))) {
      if (!latestByConductor.has(ping.conductorId)) {
        latestByConductor.set(ping.conductorId, ping);
      }
    }

    const conductores = await this.store.list<ConductorDoc>(COLLECTIONS.conductores);
    const vehiculos = await this.store.list<VehiculoDoc>(COLLECTIONS.vehiculos);
    const rutas = await this.store.list<RutaDoc>(COLLECTIONS.rutas);

    const result: DriverLocation[] = [];
    for (const ping of latestByConductor.values()) {
      const conductor = conductores.find((c) => c.id === ping.conductorId);
      if (!conductor) {
        continue;
      }
      const vehiculo = ping.vehiculoId != null ? vehiculos.find((v) => v.id === ping.vehiculoId) ?? null : null;
      const ruta = ping.rutaId != null ? rutas.find((r) => r.id === ping.rutaId) ?? null : null;
      result.push(this.toDriverLocation(ping, conductor, vehiculo, ruta));
    }

    return result.sort((a, b) => b.recordedAtUtc.localeCompare(a.recordedAtUtc));
  }

  async getLatest(conductorId: number): Promise<DriverLocation | null> {
    const pings = await this.store.list<PingDoc>(COLLECTIONS.pings);
    const latest = pings
      .filter((p) => p.conductorId === conductorId)
      .sort((a, b) => b.recordedAtUtc.localeCompare(a.recordedAtUtc))[0];
    if (!latest) {
      return null;
    }

    const conductor = await this.store.get<ConductorDoc>(COLLECTIONS.conductores, conductorId);
    const vehiculo = latest.vehiculoId != null ? await this.store.get<VehiculoDoc>(COLLECTIONS.vehiculos, latest.vehiculoId) : null;
    const ruta = latest.rutaId != null ? await this.store.get<RutaDoc>(COLLECTIONS.rutas, latest.rutaId) : null;
    return this.toDriverLocation(latest, conductor, vehiculo, ruta);
  }

  async getHistory(conductorId: number, limit: number): Promise<DriverLocation[]> {
    const safeLimit = limit < 1 || limit > 1000 ? 200 : limit;
    const pings = await this.store.list<PingDoc>(COLLECTIONS.pings);
    const result = pings
      .filter((p) => p.conductorId === conductorId)
      .sort((a, b) => b.recordedAtUtc.localeCompare(a.recordedAtUtc))
      .slice(0, safeLimit)
      .map((p) => this.toDriverLocation(p, null, null, null));

    return result.sort((a, b) => b.recordedAtUtc.localeCompare(a.recordedAtUtc));
  }
}