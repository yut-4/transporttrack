import type { RutaDto } from '../types';

interface RutaSectionProps {
  rutas: RutaDto[];
  onOpenModal: (type: 'ruta', id?: number) => void;
  onEdit: (id: number) => void;
  onDelete: (id: number) => void;
}

export default function RutaSection({
  rutas,
  onOpenModal,
  onEdit,
  onDelete,
}: RutaSectionProps) {
  return (
    <section className="section">
      <div className="section-header">
        <h2>
          <i className="fas fa-route"></i> Rutas
        </h2>
        <button className="btn btn-primary" onClick={() => onOpenModal('ruta')}>
          <i className="fas fa-plus"></i> Nueva
        </button>
      </div>
      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Origen</th>
              <th>Destino</th>
              <th>Conductor</th>
              <th>Vehículo</th>
              <th>Fecha salida</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {rutas.map((r) => (
              <tr key={r.id}>
                <td>{r.id}</td>
                <td>{r.origen}</td>
                <td>{r.destino}</td>
                <td>{r.nombreConductor || '-'}</td>
                <td>{r.placaVehiculo || '-'}</td>
                <td>{new Date(r.fechaSalida).toLocaleDateString()}</td>
                <td className="actions">
                  <button
                    className="btn btn-sm btn-warning"
                    onClick={() => onEdit(r.id)}
                  >
                    <i className="fas fa-edit"></i>
                  </button>
                  <button
                    className="btn btn-sm btn-danger"
                    onClick={() => onDelete(r.id)}
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
