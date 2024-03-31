# SpriteShaderIntegration

This is an extension of Naninovel to help easily integrate with most popular sprite shader in AssetStore.

![](https://github.com/restush/SpriteShaderIntegration/blob/resources-branch/SSI-demo1.gif)

### Support save & load.

![](https://github.com/restush/SpriteShaderIntegration/blob/resources-branch/SSI-demo2%20-%20Made%20with%20Clipchamp.gif)

### Support rollback.

![](https://github.com/restush/SpriteShaderIntegration/blob/resources-branch/SSI-demo3%20-%20Made%20with%20Clipchamp.gif)


### Property float
````nani
@charEffect Kohaku effectFloat:_HologramFade.0.5
````
Mutiple effects of float
````nani
@charEffect Kohaku effectFloat:_HologramFade.0.5,_Shadow.0.25,_Fade.1.0
````
### Property color
The color is insensitive case.
Can type uppercase or lowercase. Can add `#` on prefix or without it (`#2d2f36` or `2d2f36`).
````nani
@charEffect Kohaku effectColor:_HologramTint.#43bf15
````
Mutiple effects of color
````nani
@charEffect Kohaku effectColor:_HologramTint.#43BF15,_ShadowColor.#2d2f36,_Tint.#02114f
````
### Property int (only 2021.1 and newer)
Most shader, use float property rather than int.
But now int is available in Unity 2021.1 and newer.
````nani
@charEffect Kohaku effectInt:_HologramFade.0
````
Mutiple effects of int
````nani
@charEffect Kohaku effectInt:_HologramFade.0,_Shadow.1,_Fade.2
````

### You can assignee multiple property with different type at same time
This will be execute `effectFloat`, `effectColor`, and `effectInt` at same time.
````nani
@charEffect Kohaku effectFloat:_HologramFade.0.5,_Shadow.0.25,_Fade.1.0 effectColor:_HologramTint.#43BF15,_ShadowColor.#2d2f36,_Tint.#02114f effectInt:_HologramFade.0,_Shadow.1,_Fade.2
````

### Control animate duration
The default transition/duration of effect is based on `Default Duration` on `Character Configuration`.\
Below `_HologramFade` will transiting with default duration.
````nani
@charEffect Kohaku effectFloat:_HologramFade.0.5
````
Below `_Shadow` will transiting with 3 second of duration.
````nani
@charEffect Kohaku effectFloat:_Shadow.0.5 time:3
````

### Tested Shader
- All In 1 Sprite Shader
- Sprite Shaders Ultimate

### Technical detail
- Supported version Naninovel 1.19 and 1.20.
- If you are using Naninovel 1.20, please add `NANINOVEL_1_20` in scripting define symbols
![](https://i.ibb.co/hDz6pH8/image.png)

### How to download
You can download Unity Package in [releases tab](https://github.com/restush/SpriteShaderIntegration/releases) or clone/download this git.

### How to install/setup
You can read documentation included in Unity Package or this cloned/downloaded git or [download here](https://github.com/restush/SpriteShaderIntegration/blob/main/Documentation%20-%20SpriteShaderIntegration%20(SSI)%20for%20Naninovel.pdf)
 
### Notes
- Using this extension could cause larger save data than normal especially enabled rollback feature and/or having many characters.
