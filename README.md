# SpriteShaderIntegration



![](https://github.com/restush/SpriteShaderIntegration/blob/resources-branch/SSI-demo1.gif)

### Support save & load.

![](https://github.com/restush/SpriteShaderIntegration/blob/resources-branch/SSI-demo2%20-%20Made%20with%20Clipchamp.gif)

### Support rollback.

![](https://github.com/restush/SpriteShaderIntegration/blob/resources-branch/SSI-demo3%20-%20Made%20with%20Clipchamp.gif)



### Scripts

#### Property float
````nani
@charEffect Kohaku effectFloat:_HologramFade.0.5
@charEffect Kohaku effectFloat:_HologramFade.0.5,_Shadow.0.25,_Fade.1.0
````

#### Property color
The color is insensitive case.
Can type uppercase or lowercase. Must write # in prefix. (Todo: next update, can type both without # or with #) 
````nani
@charEffect Kohaku effectColor:_HologramTint.#43bf15
@charEffect Kohaku effectColor:_HologramTint.#43BF15,_ShadowColor.#2d2f36,_Tint.#02114f
````

#### Property int (only 2021.1 and newer)
Most shader, use float property rather than int.
But now int is available in Unity 2021.1 and newer.
````nani
@charEffect Kohaku effectInt:_HologramFade.0
@charEffect Kohaku effectInt:_HologramFade.0,_Shadow.1,_Fade.2
````

### Tested Shader
- All In 1 Sprite Shader
- Sprite Shaders Ultimate

### How to download
You can download Unity Package in releases tab or clone this git.

### How to install/setup
You can read documentation included in Unity Package or [download here](https://github.com/restush/SpriteShaderIntegration/blob/main/Documentation%20-%20SpriteShaderIntegration%20(SSI)%20for%20Naninovel.pdf)
 
