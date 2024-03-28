    using Naninovel;
    using Naninovel.Commands;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

namespace AmoyFeels.SpriteShaderIntegration
{
    [CommandAlias("charEffect")]
    public class CharEffectSSI : ModifyCharacter
    {
        public NamedDecimalListParameter EffectFloat;
        public NamedStringListParameter EffectColor;
        public NamedIntegerListParameter EffectInt;
        public BooleanParameter EffectDefault;

        protected override UniTask ApplyAppearanceModificationAsync(ICharacterActor actor, EasingType easingType, float duration, AsyncToken asyncToken)
        {
            // Check if the character has correct implementation
            bool isEffectAssigned = (Assigned(EffectFloat) || Assigned(EffectColor) || Assigned(EffectInt) || Assigned(EffectDefault));
            if (!isEffectAssigned)
                return base.ApplyAppearanceModificationAsync(actor, easingType, duration, asyncToken);

            var spriteCharacterSSI = actor as SpriteCharacterSSI;

            if (isEffectAssigned && spriteCharacterSSI == null)
            {
                Debug.LogWarning($"You're assignee `Effect` parameter, but your character `{actor.Id}` doesn't have {nameof(SpriteCharacterSSI)} implementation. Skipped to play character effect.");
                return base.ApplyAppearanceModificationAsync(actor, easingType, duration, asyncToken);
            }

            var transitionalSprite = spriteCharacterSSI.GetTransitionalSpriteSSI();
            List<UniTask> listTasks = new List<UniTask> { base.ApplyAppearanceModificationAsync(actor, easingType, duration, asyncToken) };

            if (Assigned(EffectDefault) && EffectDefault == true)
            {
                var meta = spriteCharacterSSI.ActorMetadata.GetCustomData<CharacterMetadataSSI>();

                var propColors = meta.GetPropertyColors().Where(x => !transitionalSprite.GetColor(x.Key).Equals(x.Value)).ToDictionary(x => x.Key, x => x.Value);
                var propFloat = meta.GetPropertyFloats().Where(x => transitionalSprite.GetFloat(x.Key) != x.Value).ToDictionary(x => x.Key, x => x.Value);
                var propInt = meta.GetPropertyInt().Where(x => transitionalSprite.GetInt(x.Key) != x.Value).ToDictionary(x => x.Key, x => (float)x.Value);
                listTasks.Add(spriteCharacterSSI.ChangeEffectColors(propColors, easingType, duration, asyncToken));
                listTasks.Add(spriteCharacterSSI.ChangeEffectFloats(propFloat, easingType, duration, asyncToken));
                listTasks.Add(spriteCharacterSSI.ChangeEffectInts(propInt, easingType, duration, asyncToken));
            }
            else
            {
                if (Assigned(EffectFloat))
                {
                    var effectFloat = EffectFloat.ToDictionary(k => k.Name, v => v.NamedValue.Value);
                    listTasks.Add(spriteCharacterSSI.ChangeEffectFloats(effectFloat, easingType, duration, asyncToken));

                }
                if (Assigned(EffectColor))
                {
                    var effectColor = EffectColor.ToDictionary(k => k.Name, v => GetColor(v.Name, v.NamedValue.Value));
                    listTasks.Add(spriteCharacterSSI.ChangeEffectColors(effectColor, easingType, duration, asyncToken));
                }
                if (Assigned(EffectInt))
                {
                    var effectInt = EffectInt.ToDictionary(k => k.Name, v => (float)v.NamedValue.Value);
                    listTasks.Add(spriteCharacterSSI.ChangeEffectInts(effectInt, easingType, duration, asyncToken));
                }

            }

            return UniTask.WhenAll(listTasks);


            Color GetColor(string propertyID, string htmlColor)
            {
                Color currentColor = transitionalSprite.SpriteMaterial.GetColor(propertyID); // get current shader color
                ColorMutator currentColorMutator = new ColorMutator(currentColor); // extract color by making ColorMutator
                float defaultIntensity = currentColorMutator.exposureValue; // get intensity from ColorMutator

                bool successParseColor = ColorUtility.TryParseHtmlString(htmlColor, out Color parsedColor);
                ColorMutator newColorMutator = new ColorMutator(successParseColor ? parsedColor : currentColor);
                newColorMutator.exposureValue = defaultIntensity;

                return newColorMutator.exposureAdjustedColor;
            }
        }
    }

}