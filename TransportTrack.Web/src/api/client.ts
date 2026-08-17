import axios from 'axios';
import type {
  ConductorDto,
  ConductorModel,
  VehiculoDto,
  VehiculoModel,
  RutaDto,
  RutaModel,
} from '../types';

const api = axios.create({
  baseURL: '/api',
});

export const conductorApi = {
  getAll: () => api.get<ConductorDto[]>('/conductores').then((res) => res.data),
  getById: (id: number) =>
    api.get<ConductorDto>(`/conductores/${id}`).then((res) => res.data),
  create: (data: ConductorModel) =>
    api.post<ConductorDto>('/conductores', data).then((res) => res.data),
  update: (id: number, data: ConductorModel) =>
    api.put(`/conductores/${id}`, data).then((res) => res.data),
  delete: (id: number) =>
    api.delete(`/conductores/${id}`).then((res) => res.data),
};

export const vehiculoApi = {
  getAll: () => api.get<VehiculoDto[]>('/vehiculos').then((res) => res.data),
  getById: (id: number) =>
    api.get<VehiculoDto>(`/vehiculos/${id}`).then((res) => res.data),
  create: (data: VehiculoModel) =>
    api.post<VehiculoDto>('/vehiculos', data).then((res) => res.data),
  update: (id: number, data: VehiculoModel) =>
    api.put(`/vehiculos/${id}`, data).then((res) => res.data),
  delete: (id: number) =>
    api.delete(`/vehiculos/${id}`).then((res) => res.data),
};

export const rutaApi = {
  getAll: () => api.get<RutaDto[]>('/rutas').then((res) => res.data),
  getById: (id: number) =>
    api.get<RutaDto>(`/rutas/${id}`).then((res) => res.data),
  create: (data: RutaModel) =>
    api.post<RutaDto>('/rutas', data).then((res) => res.data),
  update: (id: number, data: RutaModel) =>
    api.put(`/rutas/${id}`, data).then((res) => res.data),
  delete: (id: number) =>
    api.delete(`/rutas/${id}`).then((res) => res.data),
};

export default api;
