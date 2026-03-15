using UnityEditor;
using UnityEngine;

public class SpriteImportSettings : AssetPostprocessor
{
    private const float DefaultPixelsPerUnit = 64f;

    void OnPreprocessTexture()
    {
        TextureImporter importer = (TextureImporter)assetImporter;

        // Считаем все текстуры спрайтами
        importer.textureType = TextureImporterType.Sprite;

        // Sprite Mode = Single
        importer.spriteImportMode = SpriteImportMode.Single;

        // Pixels Per Unit
        importer.spritePixelsPerUnit = DefaultPixelsPerUnit;

        // Filter Mode = Point (no filter)
        importer.filterMode = FilterMode.Point;

        // Wrap Mode = Clamp
        importer.wrapMode = TextureWrapMode.Clamp;

        // Compression = None
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        // Настройки для пиксель-арта
        importer.mipmapEnabled = false;
        importer.streamingMipmaps = false;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.isReadable = false;

        // Без лишних улучшений качества, которые мажут пиксели
        importer.anisoLevel = 0;

        // Для 2D обычно не нужен sRGB только если у тебя спец-логика,
        // поэтому оставляем стандартное значение.
    }
}