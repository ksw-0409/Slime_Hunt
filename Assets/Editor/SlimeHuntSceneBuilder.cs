using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 전투 씬, 프리팹, 16x16 도트 스프라이트를 한 번에 생성하는 에디터 도구
public static class SlimeHuntSceneBuilder
{
    private const string SpriteFolder = "Assets/Art/Sprites";
    private const string PrefabFolder = "Assets/Prefabs";
    private const string AnimationFolder = "Assets/Art/Animations";
    private const string ScenePath = "Assets/Scenes/Main.unity";
    private const int PixelsPerUnit = 16;

    private const float ArenaWidth = 20f;
    private const float ArenaHeight = 12f;
    private const float WallThickness = 1f;
    private const float CameraSize = 7f;

    private const int FloorSortingOrder = -10;
    private const int EffectSortingOrder = -5;
    private const int MonsterSortingOrder = 0;
    private const int PlayerSortingOrder = 1;

    private const float PlayerWalkFramesPerSecond = 8f;
    private const float SlimeWalkFramesPerSecond = 6f;
    private const string IsWalkingParameter = "IsWalking";

    // UI 도트 한 칸을 캔버스 기준 몇 단위로 그릴지 정한다
    private const float UiPixelScale = 5f;
    private const int PanelBorderPixels = 2;

    private static readonly Color SlimeTint = new Color(0.45f, 0.9f, 0.4f, 1f);
    private static readonly Color32 OutlineColor = new Color32(26, 28, 44, 255);
    private static readonly Color32 LabelColor = new Color32(148, 176, 194, 255);
    private static readonly Color32 HealthTrailColor = new Color32(255, 236, 179, 255);

    private struct UserInterfaceSprites
    {
        public Sprite White;
        public Sprite Panel;
        public Sprite Heart;
        public Sprite ScoreIcon;
    }

    // ---------------------------------------------------------------- 도트 데이터

    // 플레이어 몸통(위 13줄)은 모든 프레임이 같고 다리(아래 3줄)만 프레임마다 바뀐다
    private static readonly string[] PlayerBodyPixels =
    {
        "................",
        ".....KKKKKK.....",
        "....KRRRRRRK....",
        "...KRRRRRRRRK...",
        "...KRSSSSSSRK...",
        "...KSSESSESSK...",
        "...KSSSSSSSSK...",
        "....KSSSSSSK....",
        "...KBBBBBBBBK...",
        "..KRBBBBBBBBRK..",
        "..KRYYYYYYYYRK..",
        "..KRKBBBBBBKRK..",
        "...KKBBBBBBKK...",
    };

    // 0: 선 자세, 1: 왼발 내딛기, 2: 오른발 내딛기
    private static readonly string[][] PlayerLegFrames =
    {
        new[]
        {
            "....KNNKKNNK....",
            "....KNNKKNNK....",
            "....KKK..KKK....",
        },
        new[]
        {
            "....KNNKKNNK....",
            "...KNNK..KKK....",
            "...KKK..........",
        },
        new[]
        {
            "....KNNKKNNK....",
            "....KKK..KNNK...",
            ".........KKK....",
        },
    };

    // 사망 시 색이 회색으로 바뀌어 보이도록 슬라임은 밝은 무채색으로 그리고 프리팹에서 초록으로 물들인다
    // 0: 기본, 1: 납작하게 눌림, 2: 위로 늘어남 (바닥 줄은 모두 같은 높이)
    private static readonly string[][] SlimeFrames =
    {
        new[]
        {
            "................",
            "................",
            "................",
            "................",
            "......KKKK......",
            "....KKGGGGKK....",
            "...KGGWWGGGGK...",
            "..KGGWWGGGGGGK..",
            "..KGGGGGGGGGGK..",
            ".KGGGKGGGGKGGGK.",
            ".KGGGKGGGGKGGGK.",
            ".KGGGGGGGGGGGGK.",
            ".KDGGGGGGGGGGDK.",
            "..KDDDDDDDDDDK..",
            "...KKKKKKKKKK...",
            "................",
        },
        new[]
        {
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            ".....KKKKKK.....",
            "...KKGWWGGGGKK..",
            "..KGGWWGGGGGGGK.",
            ".KGGGGGGGGGGGGGK",
            "KGGGGKGGGGKGGGGK",
            "KGGGGGGGGGGGGGGK",
            "KDGGGGGGGGGGGGDK",
            ".KDDDDDDDDDDDDK.",
            "..KKKKKKKKKKKK..",
            "................",
        },
        new[]
        {
            "................",
            "................",
            "................",
            ".......KK.......",
            ".....KKGGKK.....",
            "....KGWWGGGK....",
            "....KGWWGGGK....",
            "...KGGGGGGGGK...",
            "...KGGGGGGGGK...",
            "...KGKGGGGKGK...",
            "...KGKGGGGKGK...",
            "...KGGGGGGGGK...",
            "...KDGGGGGGDK...",
            "....KDDDDDDK....",
            ".....KKKKKK.....",
            "................",
        },
    };

    private static readonly string[] WallPixels =
    {
        "LLLLLLLMLLLLLLLM",
        "LHLLLLLMLLLHLLLM",
        "HHHHHHHMHHHHHHHM",
        "MMMMMMMMMMMMMMMM",
        "LLLMLLLLLLLMLLLL",
        "LLLMLHLLLLLMLHLL",
        "HHHMHHHHHHHMHHHH",
        "MMMMMMMMMMMMMMMM",
        "LLLLLLLMLLLLLLLM",
        "LLLLHLLMLLLLLHLM",
        "HHHHHHHMHHHHHHHM",
        "MMMMMMMMMMMMMMMM",
        "LLLMLLLLLLLMLLLL",
        "LHLMLLLLLHLMLLLL",
        "HHHMHHHHHHHMHHHH",
        "MMMMMMMMMMMMMMMM",
    };

    private static readonly string[] GrassPixels =
    {
        "gggggggggggggggg",
        "gggLgggggggggggg",
        "ggLgLggggggdgggg",
        "gggggggggggggggg",
        "gggggggggLgggggg",
        "ggggggggLgLggggg",
        "gdgggggggggggggg",
        "gggggggggggggggg",
        "ggggggggggggLggg",
        "gggLggggggggLgLg",
        "ggLgLgggdggggggg",
        "gggggggggggggggg",
        "gggggggggggggggg",
        "ggggggdggggLgggg",
        "gggggggggggLgLgg",
        "gggggggggggggggg",
    };

    // UI 패널 테두리. 바깥 2픽셀이 9-slice 테두리가 된다
    private static readonly string[] PanelFramePixels =
    {
        ".KKKKKK.",
        "KLLLLLLK",
        "KLPPPPLK",
        "KLPPPPLK",
        "KLPPPPLK",
        "KLPPPPLK",
        "KHHHHHHK",
        ".KKKKKK.",
    };

    private static readonly string[] HeartPixels =
    {
        "................",
        "................",
        "..KKKK...KKKK...",
        ".KRRWRK.KRRRRK..",
        "KRRWWRRKRRRRRRK.",
        "KRRWRRRRRRRRRRK.",
        "KRRRRRRRRRRRRRK.",
        ".KRRRRRRRRRRRK..",
        "..KRRRRRRRRRK...",
        "...KRRRRRRRK....",
        "....KRRRRRK.....",
        ".....KRRRK......",
        "......KRK.......",
        ".......K........",
        "................",
        "................",
    };

    private static readonly Dictionary<char, Color32> Palette = new Dictionary<char, Color32>
    {
        { 'P', new Color32(41, 44, 66, 255) },
        { '.', new Color32(0, 0, 0, 0) },
        { 'K', new Color32(26, 28, 44, 255) },
        { 'R', new Color32(177, 62, 83, 255) },
        { 'S', new Color32(255, 205, 117, 255) },
        { 'E', new Color32(26, 28, 44, 255) },
        { 'B', new Color32(59, 93, 201, 255) },
        { 'Y', new Color32(244, 180, 27, 255) },
        { 'N', new Color32(107, 74, 43, 255) },
        { 'G', new Color32(220, 220, 220, 255) },
        { 'W', new Color32(255, 255, 255, 255) },
        { 'D', new Color32(150, 150, 150, 255) },
        { 'L', new Color32(148, 176, 194, 255) },
        { 'H', new Color32(86, 108, 134, 255) },
        { 'M', new Color32(51, 60, 87, 255) },
        { 'g', new Color32(62, 137, 72, 255) },
        { 'd', new Color32(38, 92, 66, 255) },
    };

    // ---------------------------------------------------------------- 진입점

    [MenuItem("Tools/Slime Hunt/Build Battle Scene")]
    public static void BuildBattleScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        Directory.CreateDirectory(SpriteFolder);
        Directory.CreateDirectory(PrefabFolder);
        Directory.CreateDirectory(AnimationFolder);
        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));

        Sprite[] playerFrames = CreatePlayerFrames();
        Sprite[] slimeFrames = CreateFrameSprites("SlimeWalk", SlimeFrames);
        RuntimeAnimatorController playerAnimator = CreateWalkAnimator("Player", playerFrames, PlayerWalkFramesPerSecond);
        RuntimeAnimatorController slimeAnimator = CreateWalkAnimator("Slime", slimeFrames, SlimeWalkFramesPerSecond);
        Sprite wallSprite = CreatePixelSprite("Wall", WallPixels, true);
        Sprite grassSprite = CreatePixelSprite("Grass", GrassPixels, true);
        Sprite effectSprite = CreateSpriteFromTexture("SkillEffect", CreateRingTexture(), false);
        Sprite whiteSprite = CreateSpriteFromTexture("White", CreateSolidTexture(4, Color.white), false);
        Sprite panelSprite = CreatePixelSprite("UIPanel", PanelFramePixels, false, new Vector4(PanelBorderPixels, PanelBorderPixels, PanelBorderPixels, PanelBorderPixels));
        Sprite heartSprite = CreatePixelSprite("UIHeart", HeartPixels, false);
        UserInterfaceSprites uiSprites = new UserInterfaceSprites
        {
            White = whiteSprite,
            Panel = panelSprite,
            Heart = heartSprite,
            ScoreIcon = slimeFrames[0],
        };

        GameObject skillEffectPrefab = CreateSkillEffectPrefab(effectSprite);
        GameObject slimePrefab = CreateSlimePrefab(slimeFrames[0], slimeAnimator);
        GameObject playerPrefab = CreatePlayerPrefab(playerFrames[0], playerAnimator, skillEffectPrefab);

        CreateMainScene(playerPrefab, slimePrefab, wallSprite, grassSprite, uiSprites);

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("[SlimeHuntSceneBuilder] Main 전투 씬 생성 완료: " + ScenePath);
    }

    // ---------------------------------------------------------------- 스프라이트

    private static Sprite[] CreatePlayerFrames()
    {
        string[][] frames = new string[PlayerLegFrames.Length][];
        for (int index = 0; index < PlayerLegFrames.Length; index++)
        {
            List<string> rows = new List<string>(PlayerBodyPixels);
            rows.AddRange(PlayerLegFrames[index]);
            frames[index] = rows.ToArray();
        }
        return CreateFrameSprites("PlayerWalk", frames);
    }

    private static Sprite[] CreateFrameSprites(string baseName, string[][] frames)
    {
        Sprite[] sprites = new Sprite[frames.Length];
        for (int index = 0; index < frames.Length; index++)
        {
            sprites[index] = CreatePixelSprite(baseName + index, frames[index], false);
        }
        return sprites;
    }

    private static Sprite CreatePixelSprite(string spriteName, string[] rows, bool isTileable, Vector4 spriteBorder = default)
    {
        int size = rows.Length;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int row = 0; row < size; row++)
        {
            string line = rows[row].PadRight(size, '.');
            for (int column = 0; column < size; column++)
            {
                Color32 color = Palette.TryGetValue(line[column], out Color32 found) ? found : Palette['.'];
                // 텍스처 좌표는 아래에서 위로 올라가므로 행을 뒤집는다
                texture.SetPixel(column, size - 1 - row, color);
            }
        }
        texture.Apply();
        return CreateSpriteFromTexture(spriteName, texture, isTileable, spriteBorder);
    }

    private static Texture2D CreateRingTexture()
    {
        const int size = 32;
        const float outerRadius = 15.5f;
        const float ringInnerRadius = 13.5f;
        const float glowInnerRadius = 12f;
        Color ringColor = new Color32(115, 239, 247, 230);
        Color glowColor = new Color32(65, 166, 246, 160);
        Color fillColor = new Color32(65, 166, 246, 60);

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                Color color = Color.clear;
                if (distance <= outerRadius)
                {
                    color = distance >= ringInnerRadius ? ringColor : distance >= glowInnerRadius ? glowColor : fillColor;
                }
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateSolidTexture(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }

    private static Sprite CreateSpriteFromTexture(string spriteName, Texture2D texture, bool isTileable, Vector4 spriteBorder = default)
    {
        string path = $"{SpriteFolder}/{spriteName}.png";
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = isTileable ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        importer.spriteBorder = spriteBorder;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        // Tiled 그리기 모드는 FullRect 메시가 필요하다
        settings.spriteMeshType = isTileable ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ---------------------------------------------------------------- 애니메이션

    // 멈춰 있을 때는 첫 프레임, IsWalking이 켜지면 걷기 프레임을 반복하는 컨트롤러를 만든다
    private static RuntimeAnimatorController CreateWalkAnimator(string characterName, Sprite[] walkFrames, float framesPerSecond)
    {
        AnimationClip idleClip = CreateSpriteClip($"{characterName}Idle", new[] { walkFrames[0] }, framesPerSecond);
        AnimationClip walkClip = CreateSpriteClip($"{characterName}Walk", walkFrames, framesPerSecond);

        string controllerPath = $"{AnimationFolder}/{characterName}.controller";
        AssetDatabase.DeleteAsset(controllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter(IsWalkingParameter, AnimatorControllerParameterType.Bool);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState idleState = stateMachine.AddState("Idle");
        idleState.motion = idleClip;
        AnimatorState walkState = stateMachine.AddState("Walk");
        walkState.motion = walkClip;
        stateMachine.defaultState = idleState;

        AddInstantTransition(idleState, walkState, AnimatorConditionMode.If);
        AddInstantTransition(walkState, idleState, AnimatorConditionMode.IfNot);
        return controller;
    }

    private static void AddInstantTransition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0f;
        transition.AddCondition(mode, 0f, IsWalkingParameter);
    }

    private static AnimationClip CreateSpriteClip(string clipName, Sprite[] frames, float framesPerSecond)
    {
        AnimationClip clip = new AnimationClip { frameRate = framesPerSecond };
        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");

        // 마지막 프레임도 한 칸만큼 보이도록 끝에 같은 프레임을 한 번 더 둔다
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frames.Length + 1];
        for (int index = 0; index <= frames.Length; index++)
        {
            keyframes[index] = new ObjectReferenceKeyframe
            {
                time = index / framesPerSecond,
                value = frames[Mathf.Min(index, frames.Length - 1)],
            };
        }
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        string clipPath = $"{AnimationFolder}/{clipName}.anim";
        AssetDatabase.DeleteAsset(clipPath);
        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }

    private static void AddWalkAnimation(GameObject target, RuntimeAnimatorController controller)
    {
        Animator animator = target.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        target.AddComponent<CharacterWalkAnimation>();
    }

    // ---------------------------------------------------------------- 프리팹

    private static GameObject CreateSkillEffectPrefab(Sprite effectSprite)
    {
        GameObject effect = new GameObject("SkillEffect");
        SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
        renderer.sprite = effectSprite;
        renderer.sortingOrder = EffectSortingOrder;

        SkillEffect skillEffect = effect.AddComponent<SkillEffect>();
        // 이펙트 원의 크기를 실제 피해 범위와 맞춘다 (스프라이트 반지름은 1유닛)
        float damageRadius = new SerializedObject(skillEffect).FindProperty("damageRadius").floatValue;
        effect.transform.localScale = Vector3.one * damageRadius;

        return SaveAsPrefab(effect, "SkillEffect");
    }

    private static GameObject CreateSlimePrefab(Sprite slimeSprite, RuntimeAnimatorController slimeAnimator)
    {
        GameObject slime = new GameObject("Slime");
        SpriteRenderer renderer = slime.AddComponent<SpriteRenderer>();
        renderer.sprite = slimeSprite;
        renderer.color = SlimeTint;
        renderer.sortingOrder = MonsterSortingOrder;

        Rigidbody2D body = CreateTopDownBody(slime);
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider = slime.AddComponent<CircleCollider2D>();
        collider.radius = 0.4f;
        collider.offset = new Vector2(0f, -0.15f);

        slime.AddComponent<MonsterHealth>();
        slime.AddComponent<MonsterChase>();
        AddWalkAnimation(slime, slimeAnimator);
        return SaveAsPrefab(slime, "Slime");
    }

    private static GameObject CreatePlayerPrefab(Sprite playerSprite, RuntimeAnimatorController playerAnimator, GameObject skillEffectPrefab)
    {
        GameObject player = new GameObject("Player");
        SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
        renderer.sprite = playerSprite;
        renderer.sortingOrder = PlayerSortingOrder;

        Rigidbody2D body = CreateTopDownBody(player);
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        // 가만히 서 있어도 몬스터 접촉 판정이 유지되도록 잠들지 않게 한다
        body.sleepMode = RigidbodySleepMode2D.NeverSleep;

        CircleCollider2D collider = player.AddComponent<CircleCollider2D>();
        collider.radius = 0.4f;
        collider.offset = new Vector2(0f, -0.1f);

        player.AddComponent<PlayerMovement>();
        player.AddComponent<PlayerHealth>();
        PlayerSkill playerSkill = player.AddComponent<PlayerSkill>();
        SetReference(playerSkill, "skillEffectPrefab", skillEffectPrefab);
        AddWalkAnimation(player, playerAnimator);

        return SaveAsPrefab(player, "Player");
    }

    private static Rigidbody2D CreateTopDownBody(GameObject target)
    {
        Rigidbody2D body = target.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        return body;
    }

    private static GameObject SaveAsPrefab(GameObject source, string prefabName)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, $"{PrefabFolder}/{prefabName}.prefab");
        Object.DestroyImmediate(source);
        return prefab;
    }

    // ---------------------------------------------------------------- 씬

    private static void CreateMainScene(GameObject playerPrefab, GameObject slimePrefab, Sprite wallSprite, Sprite grassSprite, UserInterfaceSprites uiSprites)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        CreateGlobalLight();
        CreateArena(wallSprite, grassSprite);

        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        player.transform.position = Vector3.zero;

        GameObject spawner = new GameObject("MonsterSpawner");
        MonsterSpawner monsterSpawner = spawner.AddComponent<MonsterSpawner>();
        SetReference(monsterSpawner, "monsterPrefab", slimePrefab);

        GameObject gameOverPanel = CreateUserInterface(player.GetComponent<PlayerHealth>(), uiSprites);

        GameObject gameManagerObject = new GameObject("GameManager");
        GameManager gameManager = gameManagerObject.AddComponent<GameManager>();
        SetReference(gameManager, "playerTransform", player.transform);
        SetReference(gameManager, "gameOverPanel", gameOverPanel);

        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = CameraSize;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(26, 28, 44, 255);
        cameraObject.AddComponent<AudioListener>();
    }

    private static void CreateGlobalLight()
    {
        GameObject lightObject = new GameObject("Global Light 2D");
        Light2D light = lightObject.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1f;
    }

    private static void CreateArena(Sprite wallSprite, Sprite grassSprite)
    {
        GameObject arena = new GameObject("Arena");

        GameObject floor = CreateTiledSprite("Floor", grassSprite, new Vector2(ArenaWidth, ArenaHeight), Vector2.zero, arena.transform);
        floor.GetComponent<SpriteRenderer>().sortingOrder = FloorSortingOrder;

        float halfWidth = ArenaWidth * 0.5f + WallThickness * 0.5f;
        float halfHeight = ArenaHeight * 0.5f + WallThickness * 0.5f;
        float horizontalLength = ArenaWidth + WallThickness * 2f;

        CreateWall("WallTop", wallSprite, new Vector2(horizontalLength, WallThickness), new Vector2(0f, halfHeight), arena.transform);
        CreateWall("WallBottom", wallSprite, new Vector2(horizontalLength, WallThickness), new Vector2(0f, -halfHeight), arena.transform);
        CreateWall("WallLeft", wallSprite, new Vector2(WallThickness, ArenaHeight), new Vector2(-halfWidth, 0f), arena.transform);
        CreateWall("WallRight", wallSprite, new Vector2(WallThickness, ArenaHeight), new Vector2(halfWidth, 0f), arena.transform);
    }

    private static void CreateWall(string wallName, Sprite wallSprite, Vector2 size, Vector2 position, Transform parent)
    {
        GameObject wall = CreateTiledSprite(wallName, wallSprite, size, position, parent);
        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
    }

    private static GameObject CreateTiledSprite(string objectName, Sprite sprite, Vector2 size, Vector2 position, Transform parent)
    {
        GameObject tiled = new GameObject(objectName);
        tiled.transform.SetParent(parent, false);
        tiled.transform.position = position;
        SpriteRenderer renderer = tiled.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.tileMode = SpriteTileMode.Continuous;
        renderer.size = size;
        return tiled;
    }

    // ---------------------------------------------------------------- UI

    private static GameObject CreateUserInterface(PlayerHealth playerHealth, UserInterfaceSprites sprites)
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        // 스프라이트 1픽셀이 캔버스 1단위가 되게 맞춘 뒤 UiPixelScale로 키운다
        scaler.referencePixelsPerUnit = PixelsPerUnit;

        CreateHealthPanel(canvasObject.transform, playerHealth, sprites, font);
        CreateScorePanel(canvasObject.transform, sprites, font);

        // 게임오버 패널
        RectTransform gameOverPanel = CreateImage("GameOverPanel", canvasObject.transform, sprites.White, new Color32(0, 0, 0, 180));
        StretchInside(gameOverPanel, 0f);
        Text gameOverText = CreateText("GameOverText", gameOverPanel, font, 72, TextAnchor.MiddleCenter, "GAME OVER\n\n<size=40>Press R to Restart</size>");
        StretchInside(gameOverText.rectTransform, 0f);
        gameOverPanel.gameObject.SetActive(false);

        return gameOverPanel.gameObject;
    }

    // 왼쪽 위: 하트 아이콘 + 잔상 바 + 체력 바 + 숫자
    private static void CreateHealthPanel(Transform canvas, PlayerHealth playerHealth, UserInterfaceSprites sprites, Font font)
    {
        RectTransform panel = CreatePanel("HealthPanel", canvas, sprites.Panel);
        SetAnchoredRect(panel, new Vector2(0f, 1f), new Vector2(32f, -32f), new Vector2(560f, 96f));

        RectTransform heart = CreateImage("HeartIcon", panel, sprites.Heart, Color.white);
        SetAnchoredRect(heart, new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(64f, 64f));

        RectTransform barSlot = CreateImage("HealthBarSlot", panel, sprites.White, OutlineColor);
        barSlot.anchorMin = Vector2.zero;
        barSlot.anchorMax = Vector2.one;
        barSlot.offsetMin = new Vector2(96f, 24f);
        barSlot.offsetMax = new Vector2(-24f, -24f);

        Image trailImage = CreateFilledBar("HealthTrail", barSlot, sprites.White, HealthTrailColor);
        Image fillImage = CreateFilledBar("HealthFill", barSlot, sprites.White, Color.white);

        Text healthText = CreateText("HealthText", barSlot, font, 30, TextAnchor.MiddleCenter, "100 / 100");
        StretchInside(healthText.rectTransform, 0f);
        AddTextShadow(healthText);

        PlayerHealthUI healthUI = panel.gameObject.AddComponent<PlayerHealthUI>();
        SetReference(healthUI, "playerHealth", playerHealth);
        SetReference(healthUI, "healthFillImage", fillImage);
        SetReference(healthUI, "healthTrailImage", trailImage);
        SetReference(healthUI, "healthText", healthText);
        SetGradient(healthUI, "healthColorGradient", CreateHealthGradient());
    }

    // 오른쪽 위: 슬라임 아이콘 + SCORE 라벨 + 점수 숫자
    private static void CreateScorePanel(Transform canvas, UserInterfaceSprites sprites, Font font)
    {
        RectTransform panel = CreatePanel("ScorePanel", canvas, sprites.Panel);
        SetAnchoredRect(panel, new Vector2(1f, 1f), new Vector2(-32f, -32f), new Vector2(400f, 96f));

        RectTransform icon = CreateImage("ScoreIcon", panel, sprites.ScoreIcon, SlimeTint);
        SetAnchoredRect(icon, new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(64f, 64f));

        Text label = CreateText("ScoreLabel", panel, font, 26, TextAnchor.MiddleLeft, "SCORE");
        label.color = LabelColor;
        SetAnchoredRect(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(96f, 0f), new Vector2(120f, 40f));
        AddTextShadow(label);

        // 점수가 오를 때 커졌다가 돌아오므로 오른쪽 끝을 기준점으로 둔다
        Text scoreText = CreateText("ScoreText", panel, font, 52, TextAnchor.MiddleRight, "0");
        SetAnchoredRect(scoreText.rectTransform, new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(170f, 64f));
        AddTextShadow(scoreText);

        ScoreUI scoreUI = scoreText.gameObject.AddComponent<ScoreUI>();
        SetReference(scoreUI, "scoreText", scoreText);
    }

    private static RectTransform CreatePanel(string objectName, Transform parent, Sprite panelSprite)
    {
        RectTransform panel = CreateImage(objectName, parent, panelSprite, Color.white);
        Image image = panel.GetComponent<Image>();
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = 1f / UiPixelScale;
        return panel;
    }

    private static Image CreateFilledBar(string objectName, Transform parent, Sprite whiteSprite, Color color)
    {
        RectTransform bar = CreateImage(objectName, parent, whiteSprite, color);
        StretchInside(bar, 4f);
        Image image = bar.GetComponent<Image>();
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = (int)Image.OriginHorizontal.Left;
        return image;
    }

    private static void AddTextShadow(Text text)
    {
        Shadow shadow = text.gameObject.AddComponent<Shadow>();
        shadow.effectColor = OutlineColor;
        shadow.effectDistance = new Vector2(3f, -3f);
    }

    // 도트 느낌이 나도록 섞지 않고 구간별로 끊는다: 30% 이하 빨강, 60% 이하 노랑, 그 위는 초록
    private static Gradient CreateHealthGradient()
    {
        Gradient gradient = new Gradient();
        gradient.mode = GradientMode.Fixed;
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color32(177, 62, 83, 255), 0.3f),
                new GradientColorKey(new Color32(244, 180, 27, 255), 0.6f),
                new GradientColorKey(new Color32(56, 183, 100, 255), 1f),
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return gradient;
    }

    private static RectTransform CreateImage(string objectName, Transform parent, Sprite sprite, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return (RectTransform)imageObject.transform;
    }

    private static Text CreateText(string objectName, Transform parent, Font font, int fontSize, TextAnchor alignment, string content)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = content;
        text.supportRichText = true;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static void SetAnchoredRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void StretchInside(RectTransform rect, float padding)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(padding, padding);
        rect.offsetMax = new Vector2(-padding, -padding);
    }

    // ---------------------------------------------------------------- 직렬화 도우미

    private static void SetReference(Object target, string propertyName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogError($"[SlimeHuntSceneBuilder] {target.GetType().Name}.{propertyName} 필드를 찾을 수 없습니다.");
            return;
        }
        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetGradient(Object target, string propertyName, Gradient value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogError($"[SlimeHuntSceneBuilder] {target.GetType().Name}.{propertyName} 필드를 찾을 수 없습니다.");
            return;
        }
        property.gradientValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
}
