using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainGeneratorScript : MonoBehaviour {

    //public GameObject cube;
    private int chunk;
    private float roughness;
    private bool plato;
    private bool bilinear;
    public bool renderMesh;
    public float pointSize;
    public Color gizmosColor;
    public float waitForGizmos;
    public float waitForTriangles;

    private float[,] points; 
    private int side;
    private bool dsStart = false;
    private Mesh myMesh;






    //-----------------------Основные методы-----------------------------------
    void Awake () {
        myMesh = GetComponent<MeshFilter>().mesh;
        EventManager.OnStartGeneration += StartGeneration;
    }

    //Используется, как указатель для Гизмо, что выполнение программы закончено
    void OnApplicationQuit()
    {
        dsStart = false;
        EventManager.OnStartGeneration += StartGeneration;
    }

    void Update ()
    {

    }
    //------------------------------------------------------------------------






    public void StartGeneration (Dictionary<string, object> parameters)
    {

        InitNewGenerator(parameters);     
        StartCoroutine(DiemondSquare());
    }



    //Инициализация параметров для генерации карты
    void InitNewGenerator(Dictionary<string, object> parameters)
    {
        //-----------------------Введенные параметры-------------------------------------------
        chunk = (int)parameters["chunkSize"];   //Выбранный размер чанка

        if ((string)parameters["sample"] == "Normal")   //Выбранный метод фильтрации
        {
            bilinear = false;
        }
        else
        {
            bilinear = true;
        }

        roughness = (float)parameters["roughness"];  //Шероховатость

        plato = (bool)parameters["plato"];
        //--------------------------------------------------------------------------------------


        points = new float[chunk, chunk];  //Массив точек карты. Заполняется высотами. По сути является картой высот.

        dsStart = true; //Отмечает начало алгоритма. Используется для Гизмо, при отладке.

        side = chunk;   //Текущая сторона каждого обрабатывакмого квадрата(square). На каждой итерации делится пополам.

        //Заполняем карту высот значениями по умолчанию.
        for (int x = 0; x <= chunk - 1; x++)
        {
            for (int z = 0; z <= chunk - 1; z++)
            {
                points[x, z] = -1f;
            }
        }
    }



    //Реализация алгоритма Diemond-Square
    private IEnumerator DiemondSquare()
    {
        WaitForSeconds wait = new WaitForSeconds(waitForGizmos);

        PlaseFirstSquare();
             
        while (side >= 3)
        {
            int halfSide = side / 2;

            for (int x = halfSide; x <= chunk - 1; x += side - 1)
            {
                for (int z = halfSide; z <= chunk - 1; z += side - 1)
                {
                    Square(x, z, halfSide);
                }
            }
            yield return wait;

            for (int x = halfSide; x <= chunk - 1; x += side - 1)
            {
                for (int z = halfSide; z <= chunk - 1; z += side - 1)
                {
                    Diemond(x, z, halfSide);

                }
            }
            SetMeshTrianglesInDiemond();

            yield return wait;

            side = side / 2 + 1;  
        }
    }



    //Реализация первой части алгоритма Square
    void PlaseFirstSquare()
    {

        for (int x = 0; x <= chunk-1; x += chunk-1)
        {
            for (int z = 0; z <= chunk-1; z += chunk-1)
            {
                float height;

                if (plato)
                {
                    height = 0;
                }
                else
                {
                    height = Random.Range(-HRange(), HRange());
                }

                points[x, z] = height;
            }
        }
    }



    //Реализация алгоритма Square
    void Square(int cx, int cz, int halfSide)
    {
        //4 угловые точки, по которым рассчитывается высота центральной
        float a = points[cx - halfSide, cz - halfSide]; //левый-верхний
        float b = points[cx - halfSide, cz + halfSide]; //правый-верхний
        float c = points[cx + halfSide, cz - halfSide]; //левый-нижний
        float d = points[cx + halfSide, cz + halfSide]; //правый-нижний

        //Рассчет высоты центральной точки
        float height = (a + b + c + d) / 4 + Random.Range(-HRange() * 2, HRange() * 2);

        //Установка высоты точки
        points[cx, cz] = height;
    }



    //Реализация алгоритма Diemond
    void Diemond(int cx, int cz, int halfSide)
    {
        DiemondSideExistence(cx, cz - halfSide, halfSide); //левый
        DiemondSideExistence(cx - halfSide, cz, halfSide); //верхний
        DiemondSideExistence(cx, cz + halfSide, halfSide); //правый
        DiemondSideExistence(cx + halfSide, cz, halfSide); //нижний
    }
    //Проверка: установленна ли уже точка, или нет
    void DiemondSideExistence(int cx, int cz, int halfSide)
    {
        if (plato && ((cx == 0) || (cx == chunk - 1) || 
                      (cz == 0) || (cz == chunk - 1)))
        {
            SetHeightIn0(cx, cz);
        }
        if (points[cx, cz] == -1f) //Если точка не была установлена
        {
            DiemondSide(cx, cz, halfSide); //Расчитываем высоту
        }
    }
    //Рассчитывает высоту точки
    void DiemondSide(int sx, int sz, int halfSide)
    {
        float a = 0, b = 0, c = 0, d = 0;

        int i = 0; //Всего точекб не выходящих за границы

        //Проверка выхода за границы карты, и установка 4 боковых точек, по которым рассчитывается центральная
        if (sz - halfSide >= 0)
        {
            a = points[sx, sz - halfSide];
            ++i;
        }
        if (sx - halfSide >= 0)
        {
            b = points[sx - halfSide, sz];
            ++i;
        }
        if (sz + halfSide <= chunk - 1)
        {
            c = points[sx, sz + halfSide];
            ++i;
        }
        if (sx + halfSide <= chunk - 1)
        {
            d = points[sx + halfSide, sz];
            ++i;
        }
        
        //Расчет высоты точки
        float height = (a + b + c + d) / i + Random.Range(-HRange() * 2, HRange() * 2);

        //Установка высоты точки
        points[sx, sz] = height;
    }

    void SetHeightIn0(int sx, int sz)
    {
        points[sx, sz] = 0;
    }


    //Расчет максимальной допустимой высоты точки
    private float HRange()
    {
        if (bilinear) //Если выбран пункт Bilinear, карта будет более гладкой
        {
            return side * side * roughness / chunk;
        }
        else  //Если выбран пункт Normal, карта будет бугристой
        {
            return side * roughness;
        }
    }



    //Устанавливаем точки для меша
    void SetMeshTrianglesInDiemond()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        myMesh.triangles = new int[0];
        myMesh.vertices = new Vector3[0];

        int l = 0;
        //Vector2[] uv = new Vector2[vertices.Length];

        for (int x = 0; x <= chunk - 1; x++)
        {
            for (int z = 0; z <= chunk - 1; z++)
            {
                if (points[x, z] != -1f)
                {
                    vertices.Add(new Vector3(x, points[x, z], z));
                    l++;
                    // uv[i] = new Vector2((float)x / chunk, (float)z / chunk);
                }
            }
        }
        l = Mathf.RoundToInt(Mathf.Sqrt(l));

        myMesh.vertices = vertices.ToArray();
        //myMesh.uv = uv;

        for (int x = 1, i = 0; x <= l - 1; x++, i++)
        {
            for (int z = 1; z <= l - 1; z++, i++)
            {
                triangles.Add(i);
                triangles.Add(i+1);
                triangles.Add(i+l);

                triangles.Add(i+1);
                triangles.Add(i+l+1);
                triangles.Add(i+l); 
            }
        }
        myMesh.triangles = triangles.ToArray();
        myMesh.RecalculateNormals();
    }



    //Рисуем Гизмо для отладки
    private void OnDrawGizmos()
    {
        if (dsStart)
        {
            for (int x = 0; x <= chunk - 1; x++)
            {
                for (int z = 0; z <= chunk - 1; z++)
                {
                    Gizmos.color = gizmosColor;
                    Gizmos.DrawCube(new Vector3(x, points[x, z], z), new Vector3(pointSize, pointSize, pointSize));
                }
            }
        }
    }



    











    /*
    void OnApplicationQuit()
    {
        dsStart = false;
    }



    void InitNewGenerator ()
    {
        points = new float[chunk, chunk];

        dsStart = true;

        side = chunk;

        for (int x = 0; x <= chunk - 1; x++)
        {
            for (int z = 0; z <= chunk - 1; z++)
            {
                points[x, z] = -1f;
            }
        }
    }


    void Init()
    {
        myMesh = GetComponent<MeshFilter>().mesh;
    }



    private float HRange()
    {
        if (bilinear)
        {
            return side * side * roughness * 0.05f / chunk;
        }
        else
        {
            return side * roughness / chunk;
        }
    }



    void PlaseFirstSquare ()
    {
        int _chunk = chunk - 1;

        for (int x = 0; x <= _chunk; x += _chunk)
        {
            for (int z = 0; z <= _chunk; z += _chunk)
            {
                float height;

                if (plato)
                {
                    height = 0;
                }
                else
                {
                    height = Random.Range(-L_Calc(), L_Calc());
                }

                points[x, z] = height;
            }
        }
    }



    private IEnumerator DiemondSquare ()
    {
        PlaseFirstSquare();
        WaitForSeconds wait = new WaitForSeconds(waitForGizmos);

        while (side >= 3) //if
        {
            int halfSide = side / 2;

            for (int x = halfSide; x <= chunk - 1; x += side - 1)
            {
                //yield return wait;
                for (int z = halfSide; z <= chunk - 1; z += side - 1)
                {
                    Square(x, z, halfSide);
                }
            }

            for (int x = halfSide; x <= chunk - 1; x += side - 1)
            {
                //yield return wait;
                for (int z = halfSide; z <= chunk - 1; z += side - 1)
                {
                    Diemond(x, z, halfSide);
                    
                }
            }

            side = side / 2 + 1;

            yield return wait;
        }

        if (renderMesh)
        {
            DrawMesh();
        }
    }



    void Square (int cx, int cz, int halfSide)
    {

        float a = points[cx-halfSide, cz - halfSide];
        float b = points[cx - halfSide, cz + halfSide];
        float c = points[cx + halfSide, cz - halfSide];
        float d = points[cx + halfSide, cz + halfSide];

        float height = (a + b + c + d) / 4 + Random.Range(-L_Calc() * 2, L_Calc() * 2);

        points[cx, cz] = height;
    }


    
    void Diemond (int cx, int cz, int halfSide)
    {
        DiemondSideExistence(cx, cz - halfSide, halfSide); //левый
        DiemondSideExistence(cx - halfSide, cz, halfSide); //верхний
        DiemondSideExistence(cx, cz + halfSide, halfSide); //правый
        DiemondSideExistence(cx + halfSide, cz, halfSide); //нижний
    }


    void DiemondSideExistence (int cx, int cz, int halfSide)
    {
        if (points[cx, cz] == -1f)
        {
            DiemondSide(cx, cz, halfSide);
        }
    }



    void DiemondSide (int sx, int sz, int halfSide)
    {
        float a = 0, b = 0, c = 0, d = 0;

        int i = 0;

        if (sz - halfSide >= 0)
        {
            a = points[sx, sz - halfSide];
            ++i;
        }
        if (sx - halfSide >= 0)
        {
            b = points[sx - halfSide, sz];
            ++i;
        }
        if (sz + halfSide <= chunk - 1)
        {
            c = points[sx, sz + halfSide];
            ++i;
        }
        if (sx + halfSide <= chunk - 1)
        {
            d = points[sx + halfSide, sz];
            ++i;
        }

        float height = (a + b + c + d) / i + Random.Range(-L_Calc() * 2, L_Calc() * 2);

        points[sx, sz] = height;

    }



    void SetVertices()
    {
        Vector3[] vertices = new Vector3[chunk * chunk];
        Vector2[] uv = new Vector2[vertices.Length];

        for (int x = 0, i = 0; x <= chunk - 1; x++)
        {
            for (int z = 0; z <= chunk - 1; z++, i++)
            {
                vertices[i] = new Vector3(x, points[x, z], z);
                uv[i] = new Vector2((float)x / chunk, (float)z / chunk);
            }
        }

        myMesh.vertices = vertices;
        myMesh.uv = uv;
    }


   


    void DrawMesh ()
    {
        SetVertices();
        StartCoroutine(SetTriangles());        
    }



    private void OnDrawGizmos ()
    {
        if (dsStart)
        {
            for (int x = 0; x <= chunk - 1; x++)
            {
                for (int z = 0; z <= chunk - 1; z++)
                {
                    Gizmos.color = gizmosColor;
                    Gizmos.DrawCube(new Vector3(x, points[x, z], z), new Vector3(pointSize, pointSize, pointSize));
                }
            }
        }
    }

    */
}
