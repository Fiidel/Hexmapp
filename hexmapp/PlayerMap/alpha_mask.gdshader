shader_type canvas_item;

uniform sampler2D tile_texture : repeat_enable;

uniform float tiles_to_repeat = 32.0; // how many tiles is the texture spread over
uniform float tile_size_pixels = 32.0;

varying vec2 world_position;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() {
	vec4 tile_colors = texture(tile_texture, world_position / (tiles_to_repeat * tile_size_pixels));
	float alpha = texture(TEXTURE, UV).r;
	COLOR = vec4(tile_colors.rgb, alpha);
}