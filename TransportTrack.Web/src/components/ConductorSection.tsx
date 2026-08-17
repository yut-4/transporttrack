import type { ConductorDto } from '../types';

interface ConductorSectionProps {
  conductores: ConductorDto[];
  onOpenModal: (type: 'conductor', id?: number) => void;
  onEdit: (id: number) => void;
  onDelete: (id: number) => void;
}

export default function ConductorSection({
  conductores,
  onOpenModal,
  onEdit,
  onDelete,
}: ConductorSectionProps) {
  return (
    <section className="section">
      <div className="section-header">
        <h2>
          <i className="fas fa-user-tie"></i> Conductores
        </h2>
        <button className="btn btn-primary" onClick={() => onOpenModal('conductor')}>
          <i className="fas fa-plus"></i> Nuevo
        </button>
      </div>
      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Nombre</th>
              <th>Licencia</th>
              <th>Teléfono</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {conductores.map((c) => (
              <tr key={c.id}>
                <td>{c.id}</td>
                <td>{c.nombre}</td>
                <td>{c.licencia}</td>
                <td>{c.telefono}</td>
                <td className="actions">
                  <button
                    className="btn btn-sm btn-warning"
                    onClick={() => onEdit(c.id)}
                  >
                    <i className="fas fa-edit"></i>
                  </button>
                  <button
                    className="btn btn-sm btn-danger"
                    onClick={() => onDelete(c.id)}
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
