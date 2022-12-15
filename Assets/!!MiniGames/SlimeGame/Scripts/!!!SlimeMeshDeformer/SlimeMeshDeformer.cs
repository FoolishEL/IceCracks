using System;
using System.Collections.Generic;
using UnityEngine;
using Lean;

public class SlimeMeshDeformer : MonoBehaviour
{
    public static SlimeMeshDeformer Instance;

    [HideInInspector] public bool canDeform = true;

    private Vector3[] posNew1;
    private Vector3[] posNew2;
    [HideInInspector] public Vector3[] posOrig;
    [HideInInspector] public Vector3[] posPrev;
    [HideInInspector] public Vector3[] posNew;
    [HideInInspector] public bool[] isVertexUnderFinger;
    private Vector2[] uvs;
    [HideInInspector] public Vector3[] normals;
    private Color[] colors;
    private int[] triangles;

    [HideInInspector] public Mesh mesh;
    public VectorData[] vecData;

    [Range(-5f, 5f)] public float refractionLevel;

    [Header("Physics")] [Range(0f, 1f)] public float velocityDecay = 0.01f;

    [Range(0f, 1f)] public float stretchFactor = 0.49f;

    [Range(0f, 1f)] public float physicsSpeed = 1f;

    [Range(0f, 2f)] public float tightnessFactor = 1f;

    public bool isFixedDuringSwipe = true;

    [Range(0f, 1f)] public float materialStickiness;

    [Range(0f, 1f)] public float pokeEffect;


    [Header("Finger")] [Range(0f, 10f)] public float relativeMovementConstraint = 0.2f;
    private float relativeMovementConstraintSQR;

    [Range(-3f, 3f)] public float fingerPressStrength = 0.5f;

    [Range(0f, 2f)] public float fingerStrength = 1f;

    [Range(0.2f, 4f)] public float fingerRadius = 1f;

    [Header("Flip Protection")] public bool isFlipProtection;

    [NonSerialized] public int slimeMaxX = 50;

    [NonSerialized] public int slimeMaxY = 50;

    float xSizeSQR_Multiply_relativeMovementConstraintSQR;
    float ySizeSQR_Multiply_relativeMovementConstraintSQR;

    float xSize = 10f;
    float xSizeSQR = 100f;

    float ySize = 10f;
    float ySizeSQR = 100f;

    [NonSerialized] public int slimeCenterX;

    [NonSerialized] public int slimeCenterY;

    private float curPhysicsSpeed = 1f;

    private float curPhysicsStrength;

    private Vector3 lastFingerPos;

    private bool isRefractionWasChanged;

    public Transform audioSlimeMove;

    public int targetFPS = 30;

    public bool initializeOnStart = false;
    public bool initializeOnce = false;


    private int intI;
    private int intJ;

    private bool isInitialized = false;

    public bool IsMeshTouched()
    {
        if (!canDeform)
            return false;
        return LeanTouch.Fingers.Count > 0;
    }

    private void Awake()
    {
        Instance = this;
        MeshRenderer component = GetComponent<MeshRenderer>();
        component.material = Instantiate(component.material);
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        if (initializeOnStart)
            Initialize();
    }

    public void Initialize()
    {
        if (!Application.isPlaying)
            return;
        if (isInitialized && initializeOnce)
            return;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        posNew1 = new Vector3[slimeMaxX * slimeMaxY];
        posNew2 = new Vector3[slimeMaxX * slimeMaxY];
        posOrig = new Vector3[slimeMaxX * slimeMaxY];
        vecData = new VectorData[slimeMaxX * slimeMaxY];
        colors = new Color[slimeMaxX * slimeMaxY];
        uvs = new Vector2[slimeMaxX * slimeMaxY];
        Vector2[] array = new Vector2[slimeMaxX * slimeMaxY];
        slimeCenterX = slimeMaxX / 2;
        slimeCenterY = slimeMaxY / 2;
        triangles = new int[(slimeMaxX - 1) * (slimeMaxY - 1) * 2 * 3];
        isVertexUnderFinger = new bool[slimeMaxX * slimeMaxY];
        for (int i = 0; i < slimeMaxY; i++)
        {
            for (int j = 0; j < slimeMaxX; j++)
            {
                int num = i * slimeMaxX + j;
                float z = (0f - Mathf.Sqrt(Mathf.Max(0f,
                    Mathf.Pow(slimeMaxX * 0.6f, 2f) - Mathf.Pow(j - slimeCenterX, 2f) -
                    Mathf.Pow(i - slimeCenterY, 2f)))) / slimeMaxX * xSize * 0.01f;
                posOrig[num] = new Vector3(Mathf.Lerp(0f, xSize, j / (slimeMaxX - 1f)) - xSize / 2f,
                    Mathf.Lerp(0f, ySize, i / (slimeMaxY - 1f)) - ySize / 2f, z);
                array[num] = new Vector2(Mathf.Lerp(0f, 1f, j / (slimeMaxX - 1f)),
                    Mathf.Lerp(0f, 1f, i / (slimeMaxY - 1f)));
                colors[num] = Color.blue;
            }
        }

        bool flag = false;
        int num2 = 0;
        for (int k = 0; k < slimeMaxY - 1; k++)
        {
            for (int l = 0; l < slimeMaxX - 1; l++)
            {
                int num3 = k * slimeMaxX + l;
                triangles[num2++] = num3;
                triangles[num2++] = num3 + slimeMaxX;
                triangles[num2++] = num3 + 1 + slimeMaxX;
                triangles[num2++] = num3 + 1 + slimeMaxX;
                triangles[num2++] = num3 + 1;
                triangles[num2++] = num3;
            }
        }

        for (int m = 0; m < slimeMaxY; m++)
        {
            for (int n = 0; n < slimeMaxX; n++)
            {
                int num16 = m * slimeMaxX + n;
                bool isFixed = n == 0 || m == 0 || n == slimeMaxX - 1 || m == slimeMaxY - 1;
                VectorData vectorData = new VectorData(isFixed);
                vecData[num16] = vectorData;
                AddConstraint(n, m, n + 1, m, 1f);
                AddConstraint(n, m, n, m + 1, 1f);
                AddConstraint(n, m, n + 1, m + 1, 1f);
                AddConstraint(n, m, n + 2, m, 0.7f);
                AddConstraint(n, m, n, m + 2, 0.7f);
                AddConstraint(n, m, n + 2, m + 2, 0.7f);
                vecData[n + m * slimeMaxX].ConvertToArray();
            }
        }

        for (int num17 = 0; num17 < slimeMaxY; num17++)
        {
            for (int num18 = 0; num18 < slimeMaxX; num18++)
            {
                int num19 = num17 * slimeMaxX + num18;
                posOrig[num19].z += Mathf.Sin((float)(num18 + num17) / 10f) * 0.2f;
                posNew1[num19] = (posNew2[num19] = posOrig[num19]);
            }
        }

        mesh.vertices = posOrig;
        mesh.uv = array;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        mesh.MarkDynamic();
        posNew = posNew1;
        isRefractionWasChanged = false;
        RecalculateRefractions();
        transform.localRotation = Quaternion.identity;

        u_distanceToCam = Vector3.Distance(transform.position, Camera.main.transform.position);

        isInitialized = true;
    }
    
    public void ApplyInitialForces()
    {
        for (int num20 = 0; num20 < 60; num20++)
        {
            Vector3 a = new Vector3(0f, 0f, UnityEngine.Random.Range(-0.2f, 0.2f));
            int num21 = UnityEngine.Random.Range(3, slimeMaxX - 3);
            int num22 = UnityEngine.Random.Range(3, slimeMaxY - 3);
            for (int num23 = -1; num23 <= 1; num23++)
            {
                for (int num24 = -1; num24 <= 1; num24++)
                {
                    vecData[getIndex_fromXY(num21 + num24, num22 + num23)].velocity +=
                        a / (1f + Mathf.Sqrt(num24 * num24 + num23 * num23));
                }
            }
        }
    }

    private void AddConstraint(int x1, int y1, int x2, int y2, float strength)
    {
        if (x2 >= 0 && x2 < slimeMaxX && y2 >= 0 && y2 < slimeMaxY)
        {
            int num = x1 + y1 * slimeMaxX;
            int num2 = x2 + y2 * slimeMaxX;
            vecData[x1 + y1 * slimeMaxX].AddConstraint(num2, Vector3.Distance(posOrig[num], posOrig[num2]), strength);
        }
    }

    float u_distanceToCam;
    List<LeanFinger> u_fingers;
    Vector3 u_vector2;
    Vector3 u_vector3;
    Vector3 u_fingerDistance;
    int u_num;
    int u_num2;
    int u_num3;
    int u_num4;
    Vector3 u_fingerPos;
    float u_num8;
    float u_t;
    float u_newZ;
    int u_num10;
    int u_num11;
    int u_num12;
    int u_num13;
    int u_num14;
    int u_num15;

    private void Update()
    {
        if (!canDeform)
            return;

        relativeMovementConstraintSQR = relativeMovementConstraint * relativeMovementConstraint;
        xSizeSQR_Multiply_relativeMovementConstraintSQR = xSizeSQR * relativeMovementConstraintSQR;
        ySizeSQR_Multiply_relativeMovementConstraintSQR = ySizeSQR * relativeMovementConstraintSQR;

        for (intI = 0; intI < isVertexUnderFinger.Length; intI++)
        {
            isVertexUnderFinger[intI] = false;
        }

        for (intI = 0; intI < vecData.Length; intI++)
        {
            vecData[intI].isCurrentlyFixed = vecData[intI].isPermanentlyFixed;
        }

        u_fingers = LeanTouch.Fingers;
        bool flag2 = false;
        lastFingerPos = Vector3.Lerp(audioSlimeMove.transform.position, transform.position, 0.1f);
        for (int k = 0; k < u_fingers.Count; k++)
        {
            LeanFinger leanFinger = u_fingers[k];
            u_vector2 = transform.InverseTransformPoint(leanFinger.GetLastWorldPosition(u_distanceToCam));
            u_vector3 = lastFingerPos = transform.InverseTransformPoint(leanFinger.GetWorldPosition(u_distanceToCam));
            u_fingerDistance = u_vector3 - u_vector2;
            posPrev = posNew;
            posNew = posNew != posNew1 ? posNew1 : posNew2;
            u_num = !(u_fingerDistance.x > 0f) ? 1 : -1;
            u_num2 = !(u_fingerDistance.y > 0f) ? 1 : -1;
            u_num3 = Math.Max(1, Math.Min(5, Mathf.CeilToInt(u_fingerDistance.magnitude / 0.4f)));
            for (int l = 0; l < u_num3; l++)
            {
                u_fingerPos = u_vector2 + (u_vector3 - u_vector2) * (l / (float)u_num3);
                for (int m = u_num2 > 0 ? 1 : (slimeMaxY - 2); m < slimeMaxY - 1 && m > 0; m += u_num2)
                {
                    for (int n = u_num > 0 ? 1 : slimeMaxX - 2; n < slimeMaxX - 1 && n > 0; n += u_num)
                    {
                        u_num4 = m * slimeMaxX + n;
                        FingerTouch(leanFinger, u_num4, ref posPrev[u_num4], ref u_fingerPos, ref u_fingerDistance);
                    }
                }
            }
        }

        if (flag2)
        {
            u_fingers.RemoveAt(u_fingers.Count - 1);
        }

        lastFingerPos.z = audioSlimeMove.transform.position.z;
        audioSlimeMove.transform.position = Vector3.Lerp(audioSlimeMove.transform.position, lastFingerPos, 0.3f);
        posPrev = posNew;
        posNew = ((posNew != posNew1) ? posNew1 : posNew2);
        for (intI = 0; intI < vecData.Length; intI++)
        {
            posNew[intI] = posPrev[intI];
        }

        u_num8 = (!(curPhysicsSpeed > physicsSpeed * 1.1f)) ? velocityDecay : 0.3f;
        u_t = physicsSpeed * physicsSpeed * 0.02f;

        for (u_num10 = 0; u_num10 < 1; u_num10++)
        {
            curPhysicsStrength = Mathf.Pow(curPhysicsSpeed, 4f);
            for (u_num11 = 0; u_num11 < vecData.Length; u_num11++)
            {
                if (vecData[u_num11].isCurrentlyFixed)
                {
                    vecData[u_num11].velocity = Vector3.zero;
                    continue;
                }

                vecData[u_num11].velocity.x *= 1f - u_num8;
                vecData[u_num11].velocity.y *= 1f - u_num8;
                vecData[u_num11].velocity.z *= 1f - u_num8;

                u_newZ = posNew[u_num11].z + (posOrig[u_num11].z - posNew[u_num11].z) * u_t +
                         vecData[u_num11].velocity.z;
                if (u_newZ < -6) u_newZ = -6;
                if (u_newZ > 6) u_newZ = 6;

                posNew[u_num11].x += vecData[u_num11].velocity.x;
                posNew[u_num11].y += vecData[u_num11].velocity.y;
                posNew[u_num11].z = u_newZ;
            }

            for (u_num12 = 0; u_num12 < slimeMaxY; u_num12++)
            {
                for (u_num13 = 0; u_num13 < slimeMaxX; u_num13++)
                {
                    u_num14 = slimeCenterX - u_num13;
                    if (u_num14 < 0)
                    {
                        u_num14 = slimeCenterX - u_num14;
                    }

                    u_num15 = slimeCenterY - u_num12;
                    if (u_num15 < 0)
                    {
                        u_num15 = slimeCenterY - u_num15;
                    }

                    ApplyStretch(u_num15 * slimeMaxX + u_num14);
                }
            }
        }

        RecalulateNormals();
        curPhysicsSpeed = Mathf.Lerp(physicsSpeed, curPhysicsSpeed, LerpTimeFactor(0.8f));
    }


    float LerpTimeFactor(float changeRate, float refFPS = -1f, float deltaTime = -1f)
    {
        if (refFPS == -1f)
            refFPS = targetFPS;

        if (changeRate == 0f)
            return 0f;

        if (changeRate == 1f)
            return 1f;

        if (deltaTime == -1f)
            deltaTime = Time.deltaTime;

        return 1f - Mathf.Pow(1f - changeRate, deltaTime * refFPS);
    }


    float cft_num1;
    float cft_num2;
    float cft_num3;
    float cft_num4;
    float cft_num5;
    float cft_num6;

    Vector3 cft_b1;
    Vector3 cft_b2;
    Vector3 cft_b3;
    Vector3 cft_b4;
    Vector3 cft_b5;
    Vector3 cft_b6;

    Vector3 cft_a;

    float cft_x;
    float cft_y2;
    float cft_z;
    bool cft_flag1;
    bool cft_flag2;

    private void FingerTouch(LeanFinger finger, int i, ref Vector3 pos, ref Vector3 fingerPos,
        ref Vector3 fingerDistance)
    {
        cft_num1 = ((pos.x - fingerPos.x) * (pos.x - fingerPos.x) + (pos.y - fingerPos.y) * (pos.y - fingerPos.y)) /
                   (fingerRadius * fingerRadius * 1.4f * 1.4f);
        if (cft_num1 > 1f)
        {
            posNew[i] = posPrev[i];
            return;
        }
        if (cft_num1 < 0.2f)
        {
            isVertexUnderFinger[i] = true;
        }

        cft_num1 = Mathf.Sqrt(cft_num1) * 1.4f;
        cft_num1 = 1f / (1f + Mathf.Exp(-6f * ((fingerStrength + 1f) * (1f - cft_num1) - 0.5f)));
        cft_flag1 = cft_num1 > 0.25f;

        cft_flag2 = isFixedDuringSwipe && cft_flag1;
        
        if (finger.Up)
        {
            posNew[i] = posPrev[i];
            if (cft_flag1)
            {
                cft_num2 = materialStickiness;
                float y = 0f;
                if (posNew[i].z > 0f)
                {
                    if (pokeEffect > 0.01f && cft_num1 > 0.7f)
                    {
                        y = pokeEffect * posNew[i].z * 0.05f;
                        cft_num2 -= pokeEffect;
                    }

                    cft_num2 += cft_num2 * posNew[i].z * 0.15f;
                }

                vecData[i].velocity.y += y;
                vecData[i].velocity.z += -2f * curPhysicsSpeed * cft_num2;
            }
            return;
        }


        vecData[i].isCurrentlyFixed = (vecData[i].isPermanentlyFixed | cft_flag2);
        cft_x = pos.x + fingerDistance.x * cft_num1;
        cft_y2 = pos.y + fingerDistance.y * cft_num1;
        cft_b1 = posPrev[i + 1];
        cft_b2 = posPrev[i - slimeMaxX];
        cft_b3 = posPrev[i - 1];
        cft_b4 = posPrev[i + slimeMaxX];
        cft_b5 = posPrev[i + 1 + slimeMaxX];
        cft_b6 = posPrev[i - 1 - slimeMaxX];
        cft_a.x = cft_x;
        cft_a.y = cft_y2;
        cft_a.z = pos.z;
        cft_num3 = (cft_a.x - cft_b1.x) * (cft_a.x - cft_b1.x) + (cft_a.y - cft_b1.y) * (cft_a.y - cft_b1.y);
        cft_num4 = (cft_a.x - cft_b3.x) * (cft_a.x - cft_b3.x) + (cft_a.y - cft_b3.y) * (cft_a.y - cft_b3.y);
        cft_num5 = (cft_a.x - cft_b4.x) * (cft_a.x - cft_b4.x) + (cft_a.y - cft_b4.y) * (cft_a.y - cft_b4.y);
        cft_num6 = (cft_a.x - cft_b2.x) * (cft_a.x - cft_b2.x) + (cft_a.y - cft_b2.y) * (cft_a.y - cft_b2.y);
        if (cft_num3 > xSizeSQR_Multiply_relativeMovementConstraintSQR ||
            cft_num4 > xSizeSQR_Multiply_relativeMovementConstraintSQR ||
            cft_num5 > ySizeSQR_Multiply_relativeMovementConstraintSQR ||
            cft_num6 > ySizeSQR_Multiply_relativeMovementConstraintSQR)
        {
            cft_a = pos;
        }

        cft_z = pos.z;
        cft_z = !(fingerPressStrength > 0f)
            ? Mathf.Max(3.4f * fingerPressStrength * (cft_num1 * 0.5f + 0.5f), pos.z + cft_num1 * fingerPressStrength)
            : Mathf.Min(3.4f * fingerPressStrength * (cft_num1 * 0.5f + 0.5f), pos.z + cft_num1 * fingerPressStrength);
        cft_z = cft_a.z = (6f * Mathf.Lerp(cft_z * 1f, pos.z, 0.7f) + cft_b1.z + cft_b3.z + cft_b4.z + cft_b2.z) / 10f;


        if (isFlipProtection && (
                (cft_b1.x - cft_a.x) * (cft_b2.y - cft_a.y) - (cft_b1.y - cft_a.y) * (cft_b2.x - cft_a.x) > 0f ||
                (cft_b2.x - cft_a.x) * (cft_b6.y - cft_a.y) - (cft_b2.y - cft_a.y) * (cft_b6.x - cft_a.x) > 0f ||
                (cft_b6.x - cft_a.x) * (cft_b3.y - cft_a.y) - (cft_b6.y - cft_a.y) * (cft_b3.x - cft_a.x) > 0f ||
                (cft_b3.x - cft_a.x) * (cft_b4.y - cft_a.y) - (cft_b3.y - cft_a.y) * (cft_b4.x - cft_a.x) > 0f ||
                (cft_b4.x - cft_a.x) * (cft_b5.y - cft_a.y) - (cft_b4.y - cft_a.y) * (cft_b5.x - cft_a.x) > 0f ||
                (cft_b5.x - cft_a.x) * (cft_b1.y - cft_a.y) - (cft_b5.y - cft_a.y) * (cft_b1.x - cft_a.x) > 0f)
           )
        {
            cft_a = pos;
        }

        posNew[i] = cft_a;
    }

    public int getIndex_fromXY(int x, int y)
    {
        return x + y * slimeMaxX;
    }

    Constraint[] constraints;
    int count;
    Constraint constraint;
    Vector3 v;
    float magnitude;
    float num;
    float xhalf;
    int tempIndexConstraint;

    private unsafe void ApplyStretch(int i)
    {
        float x;

        constraints = vecData[i].constraintsArr;
        count = constraints.Length;
        for (intI = 0; intI < count; intI++)
        {
            constraint = constraints[intI];
            tempIndexConstraint = constraint.i;

            v.x = posNew[tempIndexConstraint].x - posNew[i].x;
            v.y = posNew[tempIndexConstraint].y - posNew[i].y;
            v.z = posNew[tempIndexConstraint].z - posNew[i].z;

            #region Tests

            //Стандартный Vector3.magnitude - на удивление ХОРОШ
            //magnitude = 1 / v.magnitude;

            //magnitude = v.x * v.x + v.y * v.y + v.z * v.z; // для тестов

            //Поиск из 1000 заранее заготовленных пар - МЕДЛЕННО
            //for (int m = 0; m < magns.Length; m++) {
            //    if((magnitude <= magns[m].inputValue)||(m == magns.Length - 1)) {
            //        magnitude = 1 / magns[m].magnitude;
            //        break;
            //    }
            //}

            //ЕБАНЫЙ Индусский код - На 5-ти значениях очень быстро, но что толку проверять на 1000, если это пиздец?
            //if (magnitude <= 0) {
            //    magnitude = 99999;
            //}else if (magnitude <= 0.1) {
            //    magnitude = 1f / 0.33f;
            //} else if (magnitude <= 1) {
            //    magnitude = 1f / 1f;
            //} else if (magnitude <= 4) {
            //    magnitude = 1f / 2f;
            //} else if (magnitude <= 9) {
            //    magnitude = 1f / 3f;
            //} else {
            //    magnitude = 1f / 4f;
            //}


            //Попытка Упростить выше написанное -МЕДЛЕННО, работает медленно из-за массива!
            //int kkk = (int)(magnitude * 100f);
            //if (kkk > 10000) {
            //    kkk = 9999;
            //}
            //if (kkk < 0) {
            //    kkk = 0;
            //}
            //magnitude = 1 / magns[kkk].magnitude;

            //Итерационная формула Герона - МЕДЛЕННО
            //magnitude = v.x * v.x + v.y * v.y + v.z * v.z;
            //prev = 0.5f * (1 + magnitude);
            //prev = 0.5f * (prev + magnitude * (1 / prev));
            //prev = 0.5f * (prev + magnitude * (1 / prev));
            //prev = 0.5f * (prev + magnitude * (1 / prev));
            //magnitude = prev;

            //Бы́стрый обра́тный квадра́тный ко́рень - почти ИДЕАЛЕН, но unsafe

            //Бы́стрый обра́тный квадра́тный ко́рень c BitConverter - уже не unsafe, 
            //float xhalf = 0.5f * x;
            //int kkk = BitConverter.ToInt32(BitConverter.GetBytes(x), 0);
            //kkk = 0x5f3759df - (kkk >> 1);
            //x = BitConverter.ToSingle(BitConverter.GetBytes(kkk), 0);
            //magnitude = x * (1.5f - xhalf * x * x);

            #endregion

            //try { //МЕДЛЕННО
            //float x = v.sqrMagnitude; //МЕДЛЕННО
            x = v.x * v.x + v.y * v.y + v.z * v.z;
            xhalf = 0.5f * x;
            int kkk = *(int*)&x;
            kkk = 0x5f3759df - (kkk >> 1);
            x = *(float*)&kkk;
            magnitude = x * (1.5f - xhalf * x * x);
            //} catch (Exception e) {
            //magnitude =  1 / v.magnitude;
            //}

            if (magnitude > 0f)
            {
                num = (1f - constraint.restDistance * (2f - tightnessFactor) * magnitude) * stretchFactor *
                      curPhysicsStrength * constraint.strength;
                v.x *= num;
                v.y *= num;
                v.z *= num;
            }

            if (!vecData[i].isCurrentlyFixed)
            {
                vecData[i].velocity.x += v.x;
                vecData[i].velocity.y += v.y;
                vecData[i].velocity.z += v.z;
                posNew[i].x += v.x;
                posNew[i].y += v.y;
                posNew[i].z += v.z;
            }

            if (!vecData[tempIndexConstraint].isCurrentlyFixed)
            {
                vecData[tempIndexConstraint].velocity.x -= v.x;
                vecData[tempIndexConstraint].velocity.y -= v.y;
                vecData[tempIndexConstraint].velocity.z -= v.z;
                posNew[tempIndexConstraint].x -= v.x;
                posNew[tempIndexConstraint].y -= v.y;
                posNew[tempIndexConstraint].z -= v.z;
            }
        }
    }

    Vector3 rn_vec;

    private void RecalulateNormals()
    {
        mesh.vertices = posNew;
        mesh.RecalculateNormals();
        normals = mesh.normals;
        for (intI = 0; intI < normals.Length; intI++)
        {
            rn_vec = normals[intI];
            if (rn_vec.z > 0f)
            {
                //TODO Зачем это?
                normals[intI].z = -rn_vec.z;
                normals[intI].Normalize();
            }
        }

        mesh.normals = normals;
        RecalculateRefractions();
    }

    int cf_num;
    float cf_num2;
    float cf_num3;
    float cf_num4;
    float cf_num5;
    bool cf_flag;

    private void RecalculateRefractions()
    {
        cf_flag = Math.Abs(refractionLevel) > 0.01f;
        if (!cf_flag && !isRefractionWasChanged)
            return;

        for (intI = 0; intI < slimeMaxY; intI++)
        {
            for (intJ = 0; intJ < slimeMaxX; intJ++)
            {
                cf_num = intI * slimeMaxX + intJ;
                cf_num2 = (intJ != 0) ? ((posNew[cf_num].z - posNew[cf_num - 1].z) * refractionLevel) : 0f;
                cf_num3 = (intI != 0) ? ((posNew[cf_num].z - posNew[cf_num - slimeMaxX].z) * refractionLevel) : 0f;
                cf_num4 = (intJ < slimeMaxX - 1) ? ((posNew[cf_num + 1].z - posNew[cf_num].z) * refractionLevel) : 0f;
                cf_num5 = (intI < slimeMaxY - 1)
                    ? ((posNew[cf_num + slimeMaxX].z - posNew[cf_num].z) * refractionLevel)
                    : 0f;
                uvs[cf_num].x = (0f - cf_num2) * 0.5f - cf_num4 * 0.5f + (posNew[cf_num].x + xSize * 0.5f) / xSize;
                uvs[cf_num].y = (0f - cf_num3) * 0.5f - cf_num5 * 0.5f + (posNew[cf_num].y + ySize * 0.5f) / ySize;
            }
        }
        isRefractionWasChanged = cf_flag;
        mesh.uv = uvs;
    }
}