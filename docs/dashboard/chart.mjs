export function renderChart(container, state, total) {
  const width = 760, height = 270, left = 45, right = 54, top = 30, bottom = 38;
  const points = state.history;
  const start = Date.parse(points[0].at);
  const end = Math.max(start + 60_000, Date.now(), ...points.map(point => Date.parse(point.at)));
  const x = at => left + (Date.parse(at) - start) / (end - start) * (width - left - right);
  const y = value => top + (1 - value / Math.max(total, 1)) * (height - top - bottom);
  let path = `M ${x(points[0].at)} ${y(points[0].remaining)}`;
  for (const point of points.slice(1)) path += ` H ${x(point.at)} V ${y(point.remaining)}`;
  path += ` H ${width - right}`;
  const levels = [...new Set([total, Math.round(total * .75), Math.round(total * .5), Math.round(total * .25), 0])];
  const ticks = levels.map(level => `<line x1="${left}" x2="${width - right}" y1="${y(level)}" y2="${y(level)}" stroke="#303a47" ${level === 0 ? 'stroke-dasharray="5 5"' : ''}/><text x="${left - 12}" y="${y(level) + 4}" text-anchor="end">${level}</text>`).join('');
  const dots = points.map(point => `<circle cx="${x(point.at)}" cy="${y(point.remaining)}" r="3.5" fill="#8aeeee"><title>${new Date(point.at).toLocaleString()}: ${point.remaining} remaining</title></circle>`).join('');
  const last = points.at(-1);
  const date = value => new Date(value).toLocaleString(undefined, { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
  container.innerHTML = `<svg viewBox="0 0 ${width} ${height}" role="img" aria-label="Completion burndown: ${last.remaining} of ${total} requirements remain, across ${points.length} recorded observations" style="font:10px Consolas,monospace;fill:#a2afc0"><defs><linearGradient id="fill" x1="0" x2="0" y1="0" y2="1"><stop stop-color="#71dcdf" stop-opacity=".10"/><stop offset="1" stop-color="#71dcdf" stop-opacity="0"/></linearGradient></defs>${ticks}<path d="${path} V ${y(0)} H ${left} Z" fill="url(#fill)"/><path d="${path}" fill="none" stroke="#71dcdf" stroke-width="2.5"/>${dots}<circle cx="${width - right}" cy="${y(last.remaining)}" r="4" fill="#8aeeee"/><text x="${width - right + 12}" y="${y(last.remaining) + 4}" fill="#94eeee" style="font-size:13px">${last.remaining}</text><text x="${left}" y="${height - 12}">${date(start)}</text><text x="${width - right}" y="${height - 12}" text-anchor="end">Now</text>${points.length === 1 ? '<text x="380" y="140" text-anchor="middle" fill="#c3ceda">Tracking starts here.</text><text x="380" y="158" text-anchor="middle">Accept a verified requirement to record the first drop.</text>' : ''}</svg>`;
}
