import { useEffect, useState } from 'react';
import Header from './components/Header';
import Tabs from './components/Tabs';
import Modal from './components/Modal';
import ConductorSection from './components/ConductorSection';
import VehiculoSection from './components/VehiculoSection';
import RutaSection from './components/RutaSection';
import GpsPage from './features/tracking/GpsPage';
import { conductorApi, vehiculoApi, rutaApi } from './api/client';
import { demoStore } from './api/demoStore';
import type {
  ConductorDto,
  ConductorModel,
  VehiculoDto,
  VehiculoModel,
  RutaDto,
  RutaModel,
  EntityType,
  StatInfo,
} from './types';

export default function App() {
  const [activeTab, setActiveTab] = useState('dashboard');
  const [conductores, setConductores] = useState<ConductorDto[]>([]);
  const [vehiculos, setVehiculos] = useState<VehiculoDto[]>([]);
  const [rutas, setRutas] = useState<RutaDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [demoMode, setDemoMode] = useState(false);
  const [apiOnline, setApiOnline] = useState(false);

  const [modalOpen, setModalOpen] = useState(false);
  const [modalType, setModalType] = useState<EntityType>('conductor');
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editingData, setEditingData] = useState<Record<string, unknown> | null>(null);

  const loadData = async () => {
    setLoading(true);
    try {
      const [cons, vhs, rt] = await Promise.all([
        conductorApi.getAll(),
        vehiculoApi.getAll(),
        rutaApi.getAll(),
      ]);
      setConductores(cons);
      setVehiculos(vhs);
      setRutas(rt);
      setDemoMode(false);
      setApiOnline(true);
    } catch {
      setConductores(demoStore.getConductores());
      setVehiculos(demoStore.getVehiculos());
      setRutas(demoStore.getRutas());
      setDemoMode(true);
    } finally {
      setLoading(false);
    }
  };

  const checkHealth = async () => {
    try {
      const base = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '') ?? '';
      const res = await fetch(base + '/api/health', { signal: AbortSignal.timeout(4000) });
      if (res.ok) {
        setApiOnline(true);
        setDemoMode(false);
      }
    } catch {
      setApiOnline(false);
    }
  };

  useEffect(() => {
    loadData();
    const interval = setInterval(checkHealth, 15000);
    return () => clearInterval(interval);
  }, []);

  const stats: StatInfo[] = [
    { label: 'Conductores', value: conductores.length, icon: 'fas fa-user-tie' },
    { label: 'Vehículos', value: vehiculos.length, icon: 'fas fa-truck' },
    { label: 'Rutas activas', value: rutas.length, icon: 'fas fa-route' },
  ];

  const health = {
    api: demoMode ? ('demo' as const) : apiOnline ? ('online' as const) : ('offline' as const),
    gps: 'stopped' as const,
    realtime: 'off' as const,
  };

  const openModal = (type: EntityType, id?: number) => {
    setModalType(type);
    setEditingId(id ?? null);
    if (id) {
      if (type === 'conductor') {
        const c = conductores.find((x) => x.id === id);
        setEditingData(c ? { nombre: c.nombre, licencia: c.licencia, telefono: c.telefono } : null);
      } else if (type === 'vehiculo') {
        const v = vehiculos.find((x) => x.id === id);
        setEditingData(v ? { placa: v.placa, marca: v.marca, modelo: v.modelo, anio: v.anio, conductorId: v.conductorId } : null);
      } else if (type === 'ruta') {
        const r = rutas.find((x) => x.id === id);
        setEditingData(r ? { origen: r.origen, destino: r.destino, conductorId: r.conductorId, vehiculoId: r.vehiculoId, fechaSalida: r.fechaSalida, fechaLlegada: r.fechaLlegada } : null);
      }
    } else {
      setEditingData(null);
    }
    setModalOpen(true);
  };

  const closeModal = () => {
    setModalOpen(false);
    setEditingId(null);
    setEditingData(null);
  };

  const handleSubmit = async (data: Record<string, unknown>) => {
    const id = editingId;
    try {
      if (modalType === 'conductor') {
        const model: ConductorModel = {
          nombre: (data.nombre as string) || '',
          licencia: (data.licencia as string) || '',
          telefono: (data.telefono as string) || '',
        };
        if (id) {
          if (demoMode) {
            demoStore.updateConductor(id, model);
          } else {
            await conductorApi.update(id, model);
          }
        } else {
          if (demoMode) {
            demoStore.createConductor(model);
          } else {
            await conductorApi.create(model);
          }
        }
      } else if (modalType === 'vehiculo') {
        const model: VehiculoModel = {
          placa: (data.placa as string) || '',
          marca: (data.marca as string) || '',
          modelo: (data.modelo as string) || '',
          anio: Number(data.anio),
          conductorId: Number(data.conductorId),
        };
        if (id) {
          if (demoMode) {
            demoStore.updateVehiculo(id, model);
          } else {
            await vehiculoApi.update(id, model);
          }
        } else {
          if (demoMode) {
            demoStore.createVehiculo(model);
          } else {
            await vehiculoApi.create(model);
          }
        }
      } else if (modalType === 'ruta') {
        const model: RutaModel = {
          origen: (data.origen as string) || '',
          destino: (data.destino as string) || '',
          conductorId: Number(data.conductorId),
          vehiculoId: Number(data.vehiculoId),
          fechaSalida: (data.fechaSalida as string) || null,
          fechaLlegada: (data.fechaLlegada as string) || null,
        };
        if (id) {
          if (demoMode) {
            demoStore.updateRuta(id, model);
          } else {
            await rutaApi.update(id, model);
          }
        } else {
          if (demoMode) {
            demoStore.createRuta(model);
          } else {
            await rutaApi.create(model);
          }
        }
      }
      closeModal();
      await loadData();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error desconocido';
      alert('Error: ' + msg);
    }
  };

  const handleDelete = async (type: EntityType, id: number) => {
    if (!confirm('¿Está seguro de eliminar este registro?')) return;
    try {
      if (type === 'conductor') {
        if (demoMode) {
          demoStore.deleteConductor(id);
        } else {
          await conductorApi.delete(id);
        }
      } else if (type === 'vehiculo') {
        if (demoMode) {
          demoStore.deleteVehiculo(id);
        } else {
          await vehiculoApi.delete(id);
        }
      } else if (type === 'ruta') {
        if (demoMode) {
          demoStore.deleteRuta(id);
        } else {
          await rutaApi.delete(id);
        }
      }
      await loadData();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error desconocido';
      alert('Error: ' + msg);
    }
  };

  const handleEdit = (type: EntityType, id: number) => {
    openModal(type, id);
  };

  const handleOpenModal = (type: EntityType) => {
    openModal(type);
  };

  return (
    <div className="app-container">
      <Header stats={stats} health={health} />

      {demoMode && (
        <p
          style={{
            padding: '10px 16px',
            marginBottom: '20px',
            background: 'color-mix(in srgb, var(--warning) 18%, var(--surface))',
            border: '1px solid color-mix(in srgb, var(--warning) 45%, var(--surface))',
            borderRadius: '8px',
            color: 'var(--warning-text)',
            fontSize: '0.85rem',
          }}
        >
          <i className="fas fa-info-circle"></i> Modo demostración: la API no está disponible. Los datos se guardan en este navegador (localStorage).
        </p>
      )}

      <Tabs activeTab={activeTab} setActiveTab={setActiveTab} />

      {loading ? (
        <p style={{ padding: '40px', textAlign: 'center', color: 'var(--muted)' }}>
          <i className="fas fa-spinner fa-pulse"></i> Cargando datos...
        </p>
      ) : (
        <>
          {activeTab === 'dashboard' && (
            <>
              <ConductorSection
                conductores={conductores}
                onOpenModal={handleOpenModal}
                onEdit={(id) => handleEdit('conductor', id)}
                onDelete={(id) => handleDelete('conductor', id)}
              />
              <VehiculoSection
                vehiculos={vehiculos}
                onOpenModal={handleOpenModal}
                onEdit={(id) => handleEdit('vehiculo', id)}
                onDelete={(id) => handleDelete('vehiculo', id)}
              />
              <RutaSection
                rutas={rutas}
                onOpenModal={handleOpenModal}
                onEdit={(id) => handleEdit('ruta', id)}
                onDelete={(id) => handleDelete('ruta', id)}
              />
            </>
          )}

          {activeTab === 'conductores' && (
            <ConductorSection
              conductores={conductores}
              onOpenModal={handleOpenModal}
              onEdit={(id) => handleEdit('conductor', id)}
              onDelete={(id) => handleDelete('conductor', id)}
            />
          )}

          {activeTab === 'vehiculos' && (
            <VehiculoSection
              vehiculos={vehiculos}
              onOpenModal={handleOpenModal}
              onEdit={(id) => handleEdit('vehiculo', id)}
              onDelete={(id) => handleDelete('vehiculo', id)}
            />
          )}

          {activeTab === 'rutas' && (
            <RutaSection
              rutas={rutas}
              onOpenModal={handleOpenModal}
              onEdit={(id) => handleEdit('ruta', id)}
              onDelete={(id) => handleDelete('ruta', id)}
            />
          )}

          {activeTab === 'gps' && <GpsPage />}
        </>
      )}

      <footer className="footer">
        <span><i className="fas fa-truck"></i> TransportTrack</span>
        <span>Gestión de conductores, vehículos y rutas</span>
      </footer>

      <Modal
        open={modalOpen}
        onClose={closeModal}
        entityType={modalType}
        entityId={editingId}
        conductors={conductores}
        vehiculos={vehiculos}
        initialData={editingData}
        onSubmit={handleSubmit}
      />
    </div>
  );
}