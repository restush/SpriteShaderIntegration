using Naninovel;
using System.Collections.Generic;
using UnityEngine;

namespace AmoyFeels.SpriteShaderIntegration
{
    [ActorResources(typeof(Texture2D), true)]
    public class SpriteCharacterSSI : SpriteActor<CharacterMetadata>, ICharacterActor
    {
        private readonly IStateManager _stateManager;
#if NANINOVEL_1_20
        public SpriteCharacterSSI(string id, CharacterMetadata metadata, StandaloneAppearanceLoader<Texture2D> loader) : base(id, metadata, loader)
#else
        public SpriteCharacterSSI(string id, CharacterMetadata metadata) : base(id, metadata)
#endif
        {
            _stateManager = Engine.GetService<StateManager>();
            _stateManager.OnRollbackStarted += CompleteAllTweeners;
            _stateManager.OnGameSaveStarted += (s) => CompleteAllTweeners();
            _stateManager.OnGameLoadStarted += (s) => CompleteAllTweeners();
        }

        public CharacterLookDirection LookDirection
        {

#if NANINOVEL_1_20
            get => TransitionalRenderer.GetLookDirection(ActorMeta.BakedLookDirection);
            set => TransitionalRenderer.SetLookDirection(value, ActorMeta.BakedLookDirection);
#else
            get => TransitionalRenderer.GetLookDirection(ActorMetadata.BakedLookDirection);
            set => TransitionalRenderer.SetLookDirection(value, ActorMetadata.BakedLookDirection);
#endif
        }

        public UniTask ChangeLookDirectionAsync(CharacterLookDirection lookDirection, float duration,
            EasingType easingType = default, AsyncToken asyncToken = default)
        {
            return TransitionalRenderer.ChangeLookDirectionAsync(lookDirection,
#if NANINOVEL_1_20
                ActorMeta.BakedLookDirection,
#else
                ActorMetadata.BakedLookDirection,
#endif

                duration, easingType, asyncToken);
        }



        public override GameObject GameObject => gameObject;
#if NANINOVEL_1_20
#else
        protected override LocalizableResourceLoader<Texture2D> AppearanceLoader => appearanceLoader;
#endif
        protected override TransitionalRenderer TransitionalRenderer => transitionalRenderer;

        public TransitionalSpriteSSI GetTransitionalSpriteSSI()
        {
            var transitional = TransitionalRenderer as TransitionalSpriteSSI;
            if (transitional != null)
                return transitional;

            return GameObject.GetComponent<TransitionalSpriteSSI>();
        }

        // string is propertyID
        Dictionary<string, Tweener<FloatTween>> tweenerFloat = new Dictionary<string, Tweener<FloatTween>>();
        Dictionary<string, Tweener<ColorTween>> tweenerColor = new Dictionary<string, Tweener<ColorTween>>();
        Dictionary<string, Tweener<FloatTween>> tweenerInt = new Dictionary<string, Tweener<FloatTween>>();
        private LocalizableResourceLoader<Texture2D> appearanceLoader;
        TransitionalSpriteSSI transitionalRenderer;
        GameObject gameObject;

        public void CompleteAllTweeners()
        {
            foreach (var item in tweenerFloat)
            {
                if (item.Value.Running)
                    item.Value.CompleteInstantly();
            }
            tweenerFloat.Clear();

            foreach (var item in tweenerColor)
            {
                if (item.Value.Running)
                    item.Value.CompleteInstantly();
            }

            tweenerColor.Clear();

            foreach (var item in tweenerInt)
            {
                if (item.Value.Running)
                    item.Value.CompleteInstantly();
            }

            tweenerInt.Clear();

        }
        public async UniTask ChangeEffectFloats(IDictionary<string, float> propertyIDandValue, EasingType easingType, float duration, AsyncToken asyncToken)
        {
            List<UniTask> tasks = new List<UniTask>();
            if (propertyIDandValue != null)
            {
                if (tweenerFloat.Count > 0)
                {
                    foreach (var item in tweenerFloat)
                    {
                        item.Value.CompleteInstantly();
                    }
                    tweenerFloat.Clear();
                }
                foreach (var itemFloat in propertyIDandValue)
                {
                    string propertyID = itemFloat.Key;
                    float toValue = itemFloat.Value;
                    if (asyncToken.Completed || asyncToken.Canceled || _stateManager.RollbackInProgress)
                    {
                        transitionalRenderer.SetFloat(propertyID, toValue);
                        continue;
                    }
                    FloatTween floatTween = default;
                    floatTween = new FloatTween(transitionalRenderer.GetFloat(propertyID), toValue, duration, value => transitionalRenderer.SetFloat(propertyID, value), false, easingType);

                    Tweener<FloatTween> tweener = new Tweener<FloatTween>(floatTween/*, hasCompleted ? new System.Action(() => _stateManager.PushRollbackSnapshot()) : default*/);
                    this.tweenerFloat.Add(itemFloat.Key, tweener);
                    tasks.Add(tweener.RunAsync(floatTween, asyncToken, transitionalRenderer));
                }
            }

            await UniTask.WhenAll(tasks);
        }

        public async UniTask ChangeEffectInts(IDictionary<string, float> propertyIDandValue, EasingType easingType, float duration, AsyncToken asyncToken)
        {
            List<UniTask> tasks = new List<UniTask>();

            if (propertyIDandValue != null)
            {
                if (tweenerInt.Count > 0)
                {
                    foreach (var item in tweenerFloat)
                    {
                        item.Value.CompleteInstantly();
                    }
                    tweenerInt.Clear();
                }
                foreach (var itemFloat in propertyIDandValue)
                {
                    string propertyID = itemFloat.Key;
                    float toValue = itemFloat.Value;
                    if (asyncToken.Completed || asyncToken.Canceled || _stateManager.RollbackInProgress)
                    {
                        transitionalRenderer.SetFloat(propertyID, toValue);
                        continue;
                    }
                    FloatTween floatTween = default;

                    floatTween = new FloatTween(transitionalRenderer.GetInt(propertyID), toValue, duration, value => transitionalRenderer.SetInt(propertyID, (int)value), false, easingType);

                    Tweener<FloatTween> tweener = new Tweener<FloatTween>(floatTween/*, hasCompleted ? new System.Action(() => _stateManager.PushRollbackSnapshot()) : default*/);
                    this.tweenerInt.Add(itemFloat.Key, tweener);
                    tasks.Add(tweener.RunAsync(floatTween, asyncToken, transitionalRenderer));
                }
            }

            await UniTask.WhenAll(tasks);
        }

        public async UniTask ChangeEffectColors(IDictionary<string, Color> propertyIDandValue, EasingType easingType, float duration, AsyncToken asyncToken = default)
        {

            List<UniTask> tasks = new List<UniTask>();

            if (propertyIDandValue != null)
            {
                if (tweenerColor.Count > 0)
                {
                    foreach (var item in tweenerColor)
                    {
                        item.Value.CompleteInstantly();
                    }
                    tweenerColor.Clear();
                }
                foreach (var itemColor in propertyIDandValue)
                {
                    string propertyID = itemColor.Key;
                    Color toValue = itemColor.Value;
                    if (asyncToken.Completed || asyncToken.Canceled)
                    {
                        transitionalRenderer.SetColor(propertyID, toValue);
                        continue;
                    }
                    var colorTween = new ColorTween(transitionalRenderer.GetColor(propertyID), toValue, ColorTweenMode.All, duration, value => transitionalRenderer.SetColor(propertyID, value), false);
                    Tweener<ColorTween> tweener = new Tweener<ColorTween>(colorTween/*, hasCompleted ? new System.Action(() => _stateManager.PushRollbackSnapshot()) : default*/);
                    this.tweenerColor.Add(itemColor.Key, tweener);
                    tasks.Add(tweener.RunAsync(colorTween, asyncToken, transitionalRenderer));
                }
            }
            await UniTask.WhenAll(tasks);
        }

        public override UniTask InitializeAsync()
        {
            gameObject = CreateHostObject();

#if NANINOVEL_1_20
#else
            appearanceLoader = ConstructAppearanceLoader(ActorMetadata);
#endif
            AppearanceLoader.OnLocalized += HandleAppearanceLocalized;
            transitionalRenderer = CreateRuntimeSprite();
            SetVisibility(false);
            return UniTask.CompletedTask;
        }

        private TransitionalSpriteSSI CreateRuntimeSprite()
        {
            var actorObject = gameObject;
            var actorMetadata =

#if NANINOVEL_1_20
                ActorMeta;
#else
                ActorMetadata;
#endif
            var spriteRenderer = actorObject.AddComponent<TransitionalSpriteSSI>();
            var (matchMode, matchRatio) = (AspectMatchMode.Disable, 0);
            spriteRenderer.Initialize(actorMetadata.Pivot, actorMetadata.PixelsPerUnit, false, matchMode, matchRatio,
                actorMetadata.CustomTextureShader, actorMetadata.CustomSpriteShader,
#if NANINOVEL_1_20
                ActorMeta.GetCustomData<CharacterMetadataSSI>().SampleMaterial);
#else
                ActorMetadata.GetCustomData<CharacterMetadataSSI>().SampleMaterial);
#endif
            spriteRenderer.DepthPassEnabled = actorMetadata.EnableDepthPass;
            spriteRenderer.DepthAlphaCutoff = actorMetadata.DepthAlphaCutoff;
            return spriteRenderer;
        }
    }

}