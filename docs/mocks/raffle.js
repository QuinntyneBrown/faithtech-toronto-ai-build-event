import { model, save } from './data.js';
let frame=0, drawTimer=0, audio, enabled=false, device;
export const soundEnabled=()=>enabled;
export function stopEffects(){cancelAnimationFrame(frame);clearInterval(drawTimer);device?.destroy();device=undefined;}
export async function toggleSound(button){enabled=!enabled;button.textContent=enabled?'Mute sound':'Enable sound';button.setAttribute('aria-pressed',String(enabled));if(enabled){audio ||= new AudioContext();await audio.resume();tone(440);}}
function tone(frequency){if(!enabled||!audio)return;const oscillator=audio.createOscillator(),gain=audio.createGain();oscillator.frequency.value=frequency;gain.gain.setValueAtTime(0.035,audio.currentTime);gain.gain.exponentialRampToValueAtTime(0.001,audio.currentTime+0.12);oscillator.connect(gain);gain.connect(audio.destination);oscillator.start();oscillator.stop(audio.currentTime+0.13);}
export function draw(navigate){
  const eligible=model.participants.filter(p=>!model.winners.some(w=>w.id===p.id)),prize=model.prizes[model.winners.length];
  if(!eligible.length||!prize){navigate('admin-raffle',{state:'exhausted'});return;}
  navigate('admin-raffle',{state:'drawing'});
  const reduced=matchMedia('(prefers-reduced-motion: reduce)').matches;let tick=0;
  const finish=()=>{clearInterval(drawTimer);const winner=eligible[0];model.winners.push({id:winner.id,name:winner.name,prize:prize.name});save();navigate('admin-raffle',{state:'winner'});tone(660);};
  if(reduced){finish();return;}
  drawTimer=setInterval(()=>{const name=document.querySelector('#raffle-name');if(name)name.textContent=eligible[tick%eligible.length].name;tone(260+tick*12);if(++tick===20)finish();},120);
}
export async function particles(){
  let canvas=document.querySelector('#raffle-canvas');if(!canvas||matchMedia('(prefers-reduced-motion: reduce)').matches)return;
  const styles=getComputedStyle(document.documentElement),hex=styles.getPropertyValue('--cs-lime').trim();
  const color=[1,3,5].map(i=>parseInt(hex.slice(i,i+2),16)/255),start=performance.now();
  canvas.width=canvas.clientWidth*devicePixelRatio;canvas.height=canvas.clientHeight*devicePixelRatio;
  try {
    const adapter=new URLSearchParams(location.search).get('renderer')==='canvas'?null:await navigator.gpu?.requestAdapter();if(!adapter)throw new Error('Use canvas fallback');
    const localDevice=await adapter.requestDevice();if(!canvas.isConnected){localDevice.destroy();return;}device=localDevice;
    const context=canvas.getContext('webgpu'),format=navigator.gpu.getPreferredCanvasFormat();
    context.configure({device,format,alphaMode:'premultiplied'});
    const module=device.createShaderModule({code:`
      struct Clock { t:f32 }; @group(0) @binding(0) var<uniform> clock:Clock;
      struct Output { @builtin(position) position:vec4f };
      @vertex fn vertex(@builtin(vertex_index) v:u32,@builtin(instance_index) i:u32)->Output {
        let points=array<vec2f,3>(vec2f(-0.008,-0.016),vec2f(0.008,-0.016),vec2f(0.0,0.016));
        let seed=f32(i);let x=fract(sin(seed*12.9898)*43758.5453)*2.0-1.0;
        let y=1.2-fract(seed*0.137+clock.t*0.15)*2.4;
        var o:Output;o.position=vec4f(points[v]+vec2f(x+sin(clock.t+seed)*0.05,y),0,1);return o;
      }
      @fragment fn fragment()->@location(0) vec4f {return vec4f(${color.join(',')},1.0);}
    `});
    const pipeline=device.createRenderPipeline({layout:'auto',vertex:{module,entryPoint:'vertex'},fragment:{module,entryPoint:'fragment',targets:[{format}]},primitive:{topology:'triangle-list'}});
    const uniform=device.createBuffer({size:16,usage:GPUBufferUsage.UNIFORM|GPUBufferUsage.COPY_DST});
    const bind=device.createBindGroup({layout:pipeline.getBindGroupLayout(0),entries:[{binding:0,resource:{buffer:uniform}}]});
    const paint=now=>{if(!canvas.isConnected||now-start>6000)return;localDevice.queue.writeBuffer(uniform,0,new Float32Array([(now-start)/1000,0,0,0]));const encoder=localDevice.createCommandEncoder();const pass=encoder.beginRenderPass({colorAttachments:[{view:context.getCurrentTexture().createView(),clearValue:{r:0,g:0,b:0,a:0},loadOp:'clear',storeOp:'store'}]});pass.setPipeline(pipeline);pass.setBindGroup(0,bind);pass.draw(3,80);pass.end();localDevice.queue.submit([encoder.finish()]);frame=requestAnimationFrame(paint);};
    device.lost.then(()=>cancelAnimationFrame(frame));frame=requestAnimationFrame(paint);canvas.dataset.renderer='webgpu';
  } catch {
    const replacement=canvas.cloneNode();canvas.replaceWith(replacement);canvas=replacement;const ctx=canvas.getContext('2d');if(!ctx)return;
    canvas.dataset.renderer='canvas';
    const paint=now=>{if(!canvas.isConnected||now-start>6000)return;ctx.clearRect(0,0,canvas.width,canvas.height);ctx.fillStyle=hex;for(let i=0;i<60;i++){const x=((i*131)%canvas.width)+Math.sin(now/600+i)*10,y=((now-start)/5+i*43)%canvas.height;ctx.fillRect(x,y,5*devicePixelRatio,9*devicePixelRatio);}frame=requestAnimationFrame(paint);};frame=requestAnimationFrame(paint);
  }
}
