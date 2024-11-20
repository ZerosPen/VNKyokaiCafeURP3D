using DIALOGUE;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEditor;


namespace Characters
{
    public abstract class Character
    {
        public const bool Enamble_On_Start = false;
        private const float unHighLightedDarkenStrength = 0.65f;
        public const bool Default_Orientation_Is_Facing_left = true;
        public const string Animation_Refresh_Trigger = "Refresh";

        public string name = "";
        public string displayName = "";
        public RectTransform mainGame = null;
        public CharacterConfigData config;
        public Animator animator;
        public Color color { get; protected set; } = Color.white;
        protected Color displayColor => HighLighted ? HighLightColor : UnHighLightColor;
        protected Color HighLightColor => color;
        protected Color UnHighLightColor => new Color(color.r * unHighLightedDarkenStrength, color.g * unHighLightedDarkenStrength, color.b * unHighLightedDarkenStrength, color.a);
        public bool HighLighted {  get; protected set; } =true;
        protected bool facingleft = Default_Orientation_Is_Facing_left;
        public int priority { get; protected set; }

        protected CharacterManager characterManager => CharacterManager.Instance;
        public DialogController dialogController => DialogController.Instance;

        // Coroutine references
        protected Coroutine co_Showing, co_hiding, co_moving, co_ChangeColor, co_highLighting, co_flipping;

        public bool isShowing => co_Showing != null;
        public bool isHidding => co_hiding != null;
        public bool isMoving => co_moving != null;
        public bool isChangeColor => co_ChangeColor != null;
        public bool isHighLighting => (HighLighted && co_highLighting != null);
        public bool isUnHighLighting => (!HighLighted && co_highLighting != null);
        public virtual bool isVisible { get; set; }
        public bool isFacingLeft => facingleft;
        public bool isFacingRight => !facingleft;
        public bool isFlipping => co_flipping != null;

        public Character(string name, CharacterConfigData config, GameObject prefab)
        {
            this.name = name;
            displayName = name;
            this.config = config;

            if (prefab != null)
            {
                GameObject ob = Object.Instantiate(prefab, characterManager.characterPanel);
                ob.name = characterManager.FormatCharacterPath(characterManager.CharacterPerfabNameFormat, name);
                ob.SetActive(true);
                mainGame = ob.GetComponent<RectTransform>();
                animator = mainGame.GetComponentInChildren<Animator>();
            }
        }

        public Coroutine Say(string dialogue) => Say(new List<string>() { dialogue });
        
        public Coroutine Say(List<string> dialogue)
        {
            dialogController.showSpeakerName(displayName);
            UpdateTextCostumizationOnScreen();
            return dialogController.Say(dialogue);
        }

        public void SetNameFont(TMP_FontAsset font) => config.nameFont = font;
        public void SetDialogueFont(TMP_FontAsset font) => config.dialogueFont = font;
        public void SetNameColor(Color color) => config.nameColor = color;
        public void SetDialogueColor(Color color) => config.dialogueColor = color;

        public void ResetConfiguratioData() => config = CharacterManager.Instance.GetCharacterConfig(name);

        public void UpdateTextCostumizationOnScreen() => dialogController.ApplySpeakerDataToDialogContainer(config);

        public virtual Coroutine Show(float speedMultiplier = 1f)
        {
            if (isShowing)
                return co_Showing;

            if (isHidding)
                characterManager.StopCoroutine(co_hiding);

            co_Showing = characterManager.StartCoroutine(ShowOrHidingCharacter(true, speedMultiplier));
            return co_Showing;
        }

        public virtual Coroutine Hide(float speedMultiplier = 1f)
        {
            if (isHidding)
                return co_hiding;

            if (isShowing)
                characterManager.StopCoroutine(co_Showing);

            co_hiding = characterManager.StartCoroutine(ShowOrHidingCharacter(false, speedMultiplier));
            return co_hiding;
        }

        public virtual IEnumerator ShowOrHidingCharacter(bool show, float speedMultiplier = 1f)
        {
            Debug.Log("Show/Hide cannot be called from a base charType");
            yield return null;
        }

        public virtual void SetPosition(Vector2 position)
        {
            if (mainGame == null)
                return;

            (Vector2 minAnchorTarget, Vector2 maxAnchorTarget) = ConvertUITargetPositionToRelativeCharacterAnchorPointTargets(position);
            mainGame.anchorMin = minAnchorTarget;
            mainGame.anchorMax = maxAnchorTarget;
        }
        public virtual Coroutine MoveToNewPosition(Vector2 position, float speed = 2f, bool smooth = false)
        {
            if (mainGame == null)
                return null;

            if (isMoving)
                characterManager.StopCoroutine(co_moving);

            co_moving = characterManager.StartCoroutine(MovingToPosition(position, speed, smooth));

            return co_moving;
        }


        private IEnumerator MovingToPosition(Vector2 position, float speed = 2f, bool smooth = false)
        {
            (Vector2 minAnchorTarget, Vector2 maxAnchorTarget) = ConvertUITargetPositionToRelativeCharacterAnchorPointTargets(position);
            Vector2 padding = mainGame.anchorMax - mainGame.anchorMin;

            while(mainGame.anchorMin != minAnchorTarget ||  mainGame.anchorMax != maxAnchorTarget)
            {
                mainGame.anchorMin = smooth ? 
                    Vector2.Lerp(mainGame.anchorMin, minAnchorTarget, speed * Time.deltaTime)
                    : Vector2.MoveTowards(mainGame.anchorMin, minAnchorTarget, speed * Time.deltaTime * 0.35f);

                mainGame.anchorMax = mainGame.anchorMin + padding;

                if (smooth && Vector2.Distance(mainGame.anchorMin, minAnchorTarget) <= 0.001f)
                {
                    mainGame.anchorMin = minAnchorTarget;
                    mainGame.anchorMax = maxAnchorTarget;
                    break;
                }

                yield return null;
            }

            Debug.Log("Done Moving");
            co_moving = null;
        }

        protected (Vector2, Vector2) ConvertUITargetPositionToRelativeCharacterAnchorPointTargets(Vector2 position)
        {
            Vector2 padding = mainGame.anchorMax - mainGame.anchorMin;

            float maxX = 1f - padding.x;
            float maxY = 1f - padding.y;

            Vector2 minAnchorTarget = new Vector2(maxX * position.x, maxY * position.y);

            Vector2 maxAnchorTarget = minAnchorTarget + padding;

            return (minAnchorTarget, maxAnchorTarget);
        }

        public virtual void SetColor(Color color)
        {
            this.color = color;
        }

        public Coroutine TransisitioColor(Color color, float speed = 1f)
        {
            this.color = color;
            if (isChangeColor)
                characterManager.StopCoroutine(co_ChangeColor);

            co_ChangeColor = characterManager.StartCoroutine(ChangeColor(displayColor, speed));

            return co_ChangeColor;
        }

        public virtual IEnumerator ChangeColor(Color color, float speed)
        {
            Debug.Log("Color changing is not applicable on this character type!");
            yield return null;
        }

        public Coroutine HighLight(float speed = 1f, bool immadiate = false)
        {
            if(isHighLighting)
                return co_highLighting;

            if (isUnHighLighting)
                characterManager.StopCoroutine(co_highLighting);

            HighLighted = true;
            co_highLighting = characterManager.StartCoroutine(HighLighting(speed, immadiate));
            return co_highLighting;

        }
        public Coroutine UnHighLight(float speed = 1f, bool immadiate = false)
        {
            if (isUnHighLighting)
                return co_highLighting;

            if (isHighLighting)
                characterManager.StopCoroutine(co_highLighting);

            HighLighted = false;
            co_highLighting = characterManager.StartCoroutine(HighLighting(speed, immadiate));
            return co_highLighting;
        }

        public virtual IEnumerator HighLighting(float speedMultiplier, bool immadiate)
        {
            Debug.Log("Highlight is not available on this character type!");
            yield return null;
        }

        public Coroutine Flip(float speed = 1, bool immediate = false)
        {
            if (isFacingLeft)
                return FaceRight(speed, immediate);
            else
                return FaceLeft(speed, immediate);
        }

        public Coroutine FaceLeft(float speed = 1, bool  immediate = false)
        {
            if (isFlipping)
                characterManager.StopCoroutine(co_flipping);

            facingleft = true;
            co_flipping = characterManager.StartCoroutine(FaceDirection(facingleft, speed, immediate));

            return co_flipping;
        }

        public Coroutine FaceRight(float speed = 1, bool immediate = false)
        {
            if (isFlipping)
                characterManager.StopCoroutine(co_flipping);

            facingleft = false;
            co_flipping = characterManager.StartCoroutine(FaceDirection(facingleft, speed, immediate));

            return co_flipping;
        }

        public virtual IEnumerator FaceDirection(bool faceleft, float speedMultiplier, bool immediate)
        {
            Debug.Log("Cant flip on this character type!");
            yield return null;
        }

        public void SetPriority(int priority, bool autoSortCharacterOnUI = true)
        {
            this.priority = priority;

            if (autoSortCharacterOnUI)
                characterManager.SortCharacters();
        }

        public void Animate(string animation)
        {
            animator.SetTrigger(animation);
        }

        public void Animate(string animation, bool state)
        {
            animator.SetBool(animation, state);
            animator.SetTrigger(Animation_Refresh_Trigger);
        }

        public virtual void OnReceiveCastingExpression(int layer, string expression)
        {
            return;
        }
        public enum CharacterType
        {
            text,
            Sprite,
            SpriteSheet,
            Live2D,
            Model3D
        }
    }
}