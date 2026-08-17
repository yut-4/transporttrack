import { useEffect, useRef } from 'react';
import type { EntityType } from '../types';

interface ModalProps {
  open: boolean;
  onClose: () => void;
  entityType: EntityType;
  entityId?: number | null;
  conductors: { id: number; nombre: string }[];
  vehiculos: { id: number; placa: string; modelo: string }[];
  initialData?: Record<string, unknown>;
  onSubmit: (data: Record<string, unknown>) => void;
}

type FieldConfig = {
  name: string;
  label: string;
  type: string;
  options?: { value: string; label: string }[];
  required?: boolean;
};

const fieldConfigs: Record<EntityType, FieldConfig[]> = {
  conductor: [
    { name: 'nombre', label: 'Nombre completo', type: 'text', required: true },
    { name: 'licencia', label: 'Licencia', type: 'text', required: true },
    { name: 'telefono', label: 'Teléfono', type: 'tel', required: true },
  ],
  vehiculo: [
    { name: 'placa', label: 'Placa', type: 'text', required: true },
    { name: 'marca', label: 'Marca', type: 'text', required: true },
    { name: 'modelo', label: 'Modelo', type: 'text', required: true },
    { name: 'anio', label: 'Año', type: 'number', required: true },
  ],
  ruta: [
    { name: 'origen', label: 'Origen', type: 'text', required: true },
    { name: 'destino', label: 'Destino', type: 'text', required: true },
  ],
};

export default function Modal({
  open,
  onClose,
  entityType,
  entityId,
  conductors,
  vehiculos,
  initialData,
  onSubmit,
}: ModalProps) {
  const overlayRef = useRef<HTMLDivElement>(null);
  const title = entityId ? 'Editar' : 'Nuevo';
  const labels: Record<EntityType, string> = {
    conductor: 'conductor',
    vehiculo: 'vehículo',
    ruta: 'ruta',
  };

  const fields = fieldConfigs[entityType];
  const hasConductorSelect = entityType === 'vehiculo' || entityType === 'ruta';
  const hasVehiculoSelect = entityType === 'ruta';

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const form = e.currentTarget as HTMLFormElement;
    const data: Record<string, unknown> = {};
    new FormData(form).forEach((value, key) => {
      data[key] = value;
    });
    if (entityType === 'vehiculo' || entityType === 'ruta') {
      const conductorEl = form.elements.namedItem('conductorId') as HTMLSelectElement | null;
      data['conductorId'] = Number(conductorEl?.value);
    }
    if (entityType === 'ruta') {
      const vehiculoEl = form.elements.namedItem('vehiculoId') as HTMLSelectElement | null;
      data['vehiculoId'] = Number(vehiculoEl?.value);
    }
    if (entityType === 'vehiculo' || entityType === 'ruta') {
      data['anio'] = Number(data['anio']);
    }
    onSubmit(data);
  };

  const handleOverlayClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === overlayRef.current) onClose();
  };

  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    if (open) {
      document.addEventListener('keydown', handleEscape);
    }
    return () => document.removeEventListener('keydown', handleEscape);
  }, [open, onClose]);

  if (!open) return null;

  return (
    <div
      className="modal-overlay open"
      ref={overlayRef}
      onClick={handleOverlayClick}
    >
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h3>
          {title} {labels[entityType]}
        </h3>
        <div className="sub">
          {entityId ? 'Modificar datos' : 'Ingresar datos del ' + labels[entityType]}
        </div>
        <form id="modalForm" onSubmit={handleSubmit}>
          {fields.map((field) => {
            const value = initialData?.[field.name];
            return (
              <div key={field.name}>
                <label htmlFor={`field-${field.name}`}>{field.label} {field.required && '*'}</label>
                <input
                  id={`field-${field.name}`}
                  name={field.name}
                  type={field.type}
                  defaultValue={value !== undefined ? String(value ?? '') : ''}
                  required={field.required}
                  min={field.type === 'number' ? '1900' : undefined}
                  max={field.type === 'number' ? '2100' : undefined}
                />
              </div>
            );
          })}

          {hasConductorSelect && (
            <>
              <label htmlFor="conductorId">Conductor</label>
              <select id="conductorId" name="conductorId" defaultValue={String(initialData?.conductorId ?? '')}>
                {conductors.map((c) => (
                  <option key={c.id} value={c.id}>{c.nombre}</option>
                ))}
              </select>
            </>
          )}

          {hasVehiculoSelect && (
            <>
              <label htmlFor="vehiculoId">Vehículo</label>
              <select id="vehiculoId" name="vehiculoId" defaultValue={String(initialData?.vehiculoId ?? '')}>
                {vehiculos.map((v) => (
                  <option key={v.id} value={v.id}>{v.placa} · {v.modelo}</option>
                ))}
              </select>
            </>
          )}

          <div className="modal-actions">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={onClose}
            >
              <i className="fas fa-times"></i> Cancelar
            </button>
            <button type="submit" className="btn btn-success">
              <i className="fas fa-check"></i> Guardar
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
