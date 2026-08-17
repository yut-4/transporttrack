export interface ConductorDto {
  id: number;
  nombre: string;
  licencia: string;
  telefono: string;
}

export interface ConductorModel {
  nombre: string;
  licencia: string;
  telefono: string;
}

export interface VehiculoDto {
  id: number;
  placa: string;
  marca: string;
  modelo: string;
  anio: number;
  conductorId: number;
  nombreConductor: string | null;
}

export interface VehiculoModel {
  placa: string;
  marca: string;
  modelo: string;
  anio: number;
  conductorId: number;
}

export interface RutaDto {
  id: number;
  origen: string;
  destino: string;
  conductorId: number;
  nombreConductor: string | null;
  vehiculoId: number;
  placaVehiculo: string | null;
  fechaSalida: string;
  fechaLlegada: string | null;
}

export interface RutaModel {
  origen: string;
  destino: string;
  conductorId: number;
  vehiculoId: number;
  fechaSalida: string | null;
  fechaLlegada: string | null;
}

export type EntityType = 'conductor' | 'vehiculo' | 'ruta';

export interface StatInfo {
  label: string;
  value: number;
  icon: string;
}
