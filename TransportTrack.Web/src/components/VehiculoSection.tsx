import type { VehiculoDto } from '../types';

interface VehiculoSectionProps {
  vehiculos: VehiculoDto[];
  onOpenModal: (type: 'vehiculo', id?: number) => void;
  onEdit: (id: number) => void;
  onDelete: (id: number) => void;
}

export default function VehiculoSection({
  vehiculos,
  onOpenModal,
  onEdit,
  onDelete,
}: VehiculoSectionProps) {
  return (
    <section className="section">
      <div className="section-header">
        <h2>
          <i className="fas fa-truck"></i> Vehículos
        </h2>
        <button className="btn btn-primary" onClick={() => onOpenModal('vehiculo')}>
          <i className="fas fa-plus"></i> Nuevo
        </button>
      </div>
      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Placa</th>
              <th>Modelo</th>
              <th>Año</th>
              <th>Conductor</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {vehiculos.map((v) => (
              <tr key={v.id}>
                <td>{v.id}</td>
                <td>{v.placa}</td>
                <td>{v.marca} {v.modelo}</td>
                <td>{v.anio}</td>
                <td>{v.nombreConductor || '-'}</td>
                <td className="actions">
                  <button
                    className="btn btn-sm btn-warning"
                    onClick={() => onEdit(v.id)}
                  >
                    <i className="fas fa-edit"></i>
                  </button>
                  <button
                    className="btn btn-sm btn-danger"
                    onClick={() => onDelete(v.id)}
                    title="Eliminar"
                  >
                    <i className="fas fa-trash-can"></i>
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}
