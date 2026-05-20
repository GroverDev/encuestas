export function fmtFecha(s?: string | null): string {
  return s
    ? new Date(s).toLocaleDateString('es-PE', { day: '2-digit', month: 'short', year: 'numeric' })
    : '—'
}

export function fmtFechaHora(s?: string | null): string {
  return s
    ? new Date(s).toLocaleString('es-PE', { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' })
    : '—'
}
