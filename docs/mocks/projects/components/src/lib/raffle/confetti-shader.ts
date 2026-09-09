export const CONFETTI_SHADER = `
struct Uniforms { time: f32, aspect: f32, padding: vec2f, palette: array<vec4f, 3> }
@group(0) @binding(0) var<uniform> u: Uniforms;
struct Vertex { @builtin(position) position: vec4f, @location(0) color: vec4f }
fn random(n: f32) -> f32 { return fract(sin(n * 127.1 + 311.7) * 43758.5453); }
@vertex fn vertex(@builtin(vertex_index) vertex: u32, @builtin(instance_index) instance: u32) -> Vertex {
  let corners = array<vec2f, 6>(vec2f(-1,-1),vec2f(1,-1),vec2f(1,1),vec2f(-1,-1),vec2f(1,1),vec2f(-1,1));
  let i = f32(instance); let seed = random(i + 1.0);
  let angle = u.time * (2.0 + seed * 5.0) + i;
  let corner = corners[vertex] * vec2f(0.009, 0.016);
  let rotated = vec2f(corner.x*cos(angle)-corner.y*sin(angle),corner.x*sin(angle)+corner.y*cos(angle));
  let x = seed * 2.2 - 1.1 + sin(u.time * 1.8 + i) * 0.09;
  let y = 1.4 - fract(u.time * (0.18 + random(i + 3.0) * 0.12) + random(i + 9.0)) * 2.8;
  var result: Vertex;
  result.position = vec4f(vec2f(x,y) + rotated / vec2f(u.aspect,1.0),0,1);
  result.color = u.palette[instance % 3u]; return result;
}
@fragment fn fragment(input: Vertex) -> @location(0) vec4f { return input.color; }
`;
