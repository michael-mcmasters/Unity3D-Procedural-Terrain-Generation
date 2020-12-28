using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    public Texture2D perlinNoiseTexture;

    public int width = 256;
    public int height = 256;

    public float noise = 0.3f;

    public float offsetX = 100;
    public float offsetY = 100;



    private static PerlinNoise instance;
    public static PerlinNoise Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<PerlinNoise>();

            return instance;
        }
    }






    protected void Start()
    {
        offsetX = Random.Range(0, 99999);
        offsetY = Random.Range(0, 99999);

        Debug.Log("Perlin Noise Texture is updating each frame. This is useful for debugging noise. But disable the gameobject or class to stop.");
    }


#if (UNITY_EDITOR)
    protected void Update()
    {
        GeneratePerlinNoise();
    }
#endif



    // Uses perlin noise and assigns it to a texture. Black/white and any gradient in-between mean different things.
    public Texture2D GeneratePerlinNoise()
    {
        perlinNoiseTexture = GenerateTexture();
        return perlinNoiseTexture;
    }

    public Texture2D GeneratePerlinNoise(int width, int height, float noise)
    {
        this.width = width;
        this.height = height;
        this.noise = noise;

        perlinNoiseTexture = GenerateTexture();
        return perlinNoiseTexture;
    }



    private Texture2D GenerateTexture()
    {
        offsetX = Random.Range(0, 99999);
        offsetY = Random.Range(0, 99999);

        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = perlinNoiseTexture;


        Texture2D texture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = CalculateColor(x, y);

                // ToDo: Change this to SetPixels() (plural) by creating an array and passing it in. SetPixel() is really slow.
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    private Color CalculateColor(int x, int y)
    {
        float xCoord = (float)x / width * noise + offsetX;
        float yCoord = (float)y / height * noise + offsetY;

        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(sample, sample, sample);
    }

}