import { CONFETTI_SHADER } from './confetti-shader';
export async function startConfetti(canvas: HTMLCanvasElement, lost: () => void): Promise<() => void> {
  if (!navigator.gpu) throw new Error('GPU unavailable');
  const adapter = await navigator.gpu.requestAdapter(); if (!adapter) throw new Error('GPU adapter unavailable');
  const device = await adapter.requestDevice(); let stopped = false, frame = 0;
  const stop = () => { stopped = true; cancelAnimationFrame(frame); device.destroy(); };
  try {
    const context = canvas.getContext('webgpu'); if (!context) throw new Error('GPU canvas unavailable');
    const format = navigator.gpu.getPreferredCanvasFormat();
    context.configure({ device, format, alphaMode: 'premultiplied' });
    const module = device.createShaderModule({ code: CONFETTI_SHADER });
    const pipeline = await device.createRenderPipelineAsync({ layout: 'auto', vertex: { module, entryPoint: 'vertex' }, fragment: { module, entryPoint: 'fragment', targets: [{ format }] }, primitive: { topology: 'triangle-list' } });
    const buffer = device.createBuffer({ size: 64, usage: GPUBufferUsage.UNIFORM | GPUBufferUsage.COPY_DST });
    const bindings = device.createBindGroup({ layout: pipeline.getBindGroupLayout(0), entries: [{ binding: 0, resource: { buffer } }] });
    const values = new Float32Array(16), styles = getComputedStyle(canvas);
    ['--cs-lime', '--cs-success', '--cs-ink'].forEach((role, index) => {
      const hex = styles.getPropertyValue(role).trim().replace('#', '');
      const color = parseInt(hex, 16);
      values.set([((color >> 16) & 255)/255,((color >> 8) & 255)/255,(color & 255)/255,1],4 + index * 4);
    });
    const started = performance.now();
    const render = (now: number) => {
      if (stopped) return;
      try {
        const bounds = canvas.getBoundingClientRect(), scale = Math.min(devicePixelRatio, 2);
        const width = Math.max(1, Math.round(bounds.width * scale)), height = Math.max(1, Math.round(bounds.height * scale));
        if (canvas.width !== width || canvas.height !== height) { canvas.width = width; canvas.height = height; }
        values[0] = (now-started)/1000; values[1] = width/height; device.queue.writeBuffer(buffer,0,values);
        const encoder = device.createCommandEncoder();
        const pass = encoder.beginRenderPass({ colorAttachments: [{ view: context.getCurrentTexture().createView(), clearValue: { r:0,g:0,b:0,a:0 }, loadOp:'clear', storeOp:'store' }] });
        pass.setPipeline(pipeline); pass.setBindGroup(0,bindings); pass.draw(6,180); pass.end(); device.queue.submit([encoder.finish()]);
        frame = requestAnimationFrame(render);
      } catch { stop(); lost(); }
    };
    void device.lost.then(() => { if (!stopped) { stopped = true; cancelAnimationFrame(frame); lost(); } });
    frame = requestAnimationFrame(render); return stop;
  } catch (error) { stop(); throw error; }
}
