import type {
  ConductorDto,
  ConductorModel,
  VehiculoDto,
  VehiculoModel,
  RutaDto,
  RutaModel,
} from '../types';

const CONDUCTORES_KEY = 'tt_conductores';
const VEHICULOS_KEY = 'tt_vehiculos';
const RUTAS_KEY = 'tt_rutas';

const seedConductores: ConductorDto[] = [
  { id: 1, nombre: 'Ana Martínez', licencia: 'LIC-001', telefono: '+34 611 22 33 44' },
  { id: 2, nombre: 'Carlos Gómez', licencia: 'LIC-002', telefono: '+34 622 55 66 77' },
];

const seedVehiculos: VehiculoDto[] = [
  { id: 1, placa: 'ABC-1234', marca: 'Mercedes', modelo: 'Sprinter', anio: 2022, conductorId: 1, nombreConductor: 'Ana Martínez' },
  { id: 2, placa: 'XYZ-9876', marca: 'Ford', modelo: 'Transit', anio: 2021, conductorId: 2, nombreConductor: 'Carlos Gómez' },
];

const seedRutas: RutaDto[] = [
  {
    id: 1,
    origen: 'Madrid',
    destino: 'Barcelona',
    conductorId: 1,
    nombreConductor: 'Ana Martínez',
    vehiculoId: 1,
    placaVehiculo: 'ABC-1234',
    fechaSalida: new Date().toISOString(),
    fechaLlegada: null,
  },
  {
    id: 2,
    origen: 'Valencia',
    destino: 'Sevilla',
    conductorId: 2,
    nombreConductor: 'Carlos Gómez',
    vehiculoId: 2,
    placaVehiculo: 'XYZ-9876',
    fechaSalida: new Date().toISOString(),
    fechaLlegada: null,
  },
];

function read<T>(key: string, seed: T[]): T[] {
  const raw = localStorage.getItem(key);
  if (!raw) {
    localStorage.setItem(key, JSON.stringify(seed));
    return [...seed];
  }
  try {
    return JSON.parse(raw) as T[];
  } catch {
    return [...seed];
  }
}

function write<T>(key: string, data: T[]) {
  localStorage.setItem(key, JSON.stringify(data));
}

function nextId<T extends { id: number }>(items: T[]): number {
  return items.length > 0 ? Math.max(...items.map((i) => i.id)) + 1 : 1;
}

export const demoStore = {
  getConductores: () => read<ConductorDto>(CONDUCTORES_KEY, seedConductores),
  getVehiculos: () => read<VehiculoDto>(VEHICULOS_KEY, seedVehiculos),
  getRutas: () => read<RutaDto>(RUTAS_KEY, seedRutas),

  createConductor(data: ConductorModel): ConductorDto {
    const items = this.getConductores();
    const nuevo: ConductorDto = { id: nextId(items), ...data };
    write(CONDUCTORES_KEY, [...items, nuevo]);
    return nuevo;
  },

  updateConductor(id: number, data: ConductorModel) {
    const items = this.getConductores();
    write(
      CONDUCTORES_KEY,
      items.map((c) => (c.id === id ? { ...c, ...data } : c)),
    );
  },

  deleteConductor(id: number) {
    write(CONDUCTORES_KEY, this.getConductores().filter((c) => c.id !== id));
  },

  createVehiculo(data: VehiculoModel): VehiculoDto {
    const items = this.getVehiculos();
    const conductor = this.getConductores().find((c) => c.id === data.conductorId);
    const nuevo: VehiculoDto = {
      id: nextId(items),
      ...data,
      nombreConductor: conductor?.nombre ?? null,
    };
    write(VEHICULOS_KEY, [...items, nuevo]);
    return nuevo;
  },

  updateVehiculo(id: number, data: VehiculoModel) {
    const items = this.getVehiculos();
    const conductor = this.getConductores().find((c) => c.id === data.conductorId);
    write(
      VEHICULOS_KEY,
      items.map((v) => (v.id === id ? { ...v, ...data, nombreConductor: conductor?.nombre ?? null } : v)),
    );
  },

  deleteVehiculo(id: number) {
    write(VEHICULOS_KEY, this.getVehiculos().filter((v) => v.id !== id));
  },

  createRuta(data: RutaModel): RutaDto {
    const items = this.getRutas();
    const conductor = this.getConductores().find((c) => c.id === data.conductorId);
    const vehiculo = this.getVehiculos().find((v) => v.id === data.vehiculoId);
    const nuevo: RutaDto = {
      id: nextId(items),
      ...data,
      fechaSalida: data.fechaSalida ?? new Date().toISOString(),
      nombreConductor: conductor?.nombre ?? null,
      placaVehiculo: vehiculo?.placa ?? null,
    };
    write(RUTAS_KEY, [...items, nuevo]);
    return nuevo;
  },

  updateRuta(id: number, data: RutaModel) {
    const items = this.getRutas();
    const conductor = this.getConductores().find((c) => c.id === data.conductorId);
    const vehiculo = this.getVehiculos().find((v) => v.id === data.vehiculoId);
    write(
      RUTAS_KEY,
      items.map((r) =>
        r.id === id
          ? {
              ...r,
              ...data,
              fechaSalida: data.fechaSalida ?? r.fechaSalida,
              nombreConductor: conductor?.nombre ?? null,
              placaVehiculo: vehiculo?.placa ?? null,
            }
          : r,
      ),
    );
  },

  deleteRuta(id: number) {
    write(RUTAS_KEY, this.getRutas().filter((r) => r.id !== id));
  },
};