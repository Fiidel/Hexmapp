shader_type canvas_item;

uniform sampler2D tile_texture : repeat_enable;
uniform sampler2D noise : repeat_enable;
uniform float tiles_to_repeat = 32.0; // how many tiles is the texture spread over
uniform float tile_size_pixels = 32.0;
uniform float noise_scale = 8; // how many tiles before noise repeats
varying vec2 world_position;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() {
	// get the terrain color
	vec2 terrain_world_uv = world_position / (tiles_to_repeat * tile_size_pixels);
	vec2 noise_world_uv = world_position / (noise_scale * tile_size_pixels);
	vec4 tile_colors = texture(tile_texture, terrain_world_uv);
	
	// map the normalized noise from (0,1) to (0.3,1)
	float noise_value = texture(noise, noise_world_uv).r * 0.7 + 0.3;
	
	// get the alpha value from the alpha mask tiles
	float tile_alpha = texture(TEXTURE, UV).r;
	
	// the tile alpha mask is multiplied by the remapped noise value
	// the alpha mask has a value of 1 (white) where the tile texture *must* be rendered (to avoid blank spaces)
		// meaning those pixels will have alpha values of 0.3-1, so those we always render
	// then the alpha mask tiles have have gradients of values less than 1, so multiplied
	// by the noise value, they attain values in the interval (0,1)
	float alpha_total = tile_alpha * noise_value;
	
	// threshold using the alpha mask tiles and the noise
	if (alpha_total > 0.3)
	{
		COLOR = vec4(tile_colors.rgb, clamp(alpha_total+0.7, 0.0, 1.0));
	}
	else
	{
		COLOR = vec4(0, 0, 0, 0);
	}
}
