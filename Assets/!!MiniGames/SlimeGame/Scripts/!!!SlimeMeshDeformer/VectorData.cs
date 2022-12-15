using System.Collections.Generic;
using UnityEngine;

public class VectorData {
    public bool isPermanentlyFixed;

    public bool isCurrentlyFixed;

    //public bool isLastFixed;

    //public bool isFlipped;

    public Vector3 velocity;

    public List<Constraint> constraints;

    public Constraint[] constraintsArr;

    public VectorData(bool isFixed) {
        isPermanentlyFixed = isFixed;
        constraints = new List<Constraint>();
    }

    public void AddConstraint(int i, float restDistance, float strength) {
        constraints.Add(new Constraint(i, restDistance, strength));
    }

    public void ConvertToArray() {
        constraintsArr = constraints.ToArray();
    }


}


/*

private void CalculateFingerTouch_2(LeanFinger finger, int i, ref Vector3 pos, ref Vector3 fingerPos, ref Vector3 fingerDistance) {
    float num = ((pos.x - fingerPos.x) * (pos.x - fingerPos.x) + (pos.y - fingerPos.y) * (pos.y - fingerPos.y)) / (fingerRadius * fingerRadius * 1.4f * 1.4f);
    if (num > 1f) {
        newPos[i] = prevPos[i];
        return;
    }
    num = Mathf.Sqrt(num) * 1.4f;
    num = 1f / (1f + Mathf.Exp(-6f * ((fingerStrength + 1f) * (1f - num) - 0.5f)));
    bool flag = num > 0.25f;
    bool flag2 = isFixedDuringSwipe && flag;
    if (finger.Up) {
        newPos[i] = prevPos[i];
        if (flag) {
            float num2 = materialStickiness;
            float y = 0f;
            if (newPos[i].z > 0f) {
                if (pokeEffect > 0.01f && num > 0.7f) {
                    y = pokeEffect * newPos[i].z * 0.05f;
                    num2 -= pokeEffect;
                }
                num2 += num2 * newPos[i].z * 0.15f;
            }
            vecData[i].velocity += new Vector3(0f, y, -2f * currentPhysicsSpeed * num2);
        }
        return;
    }

    vecData[i].isCurrentlyFixed = (vecData[i].isPermanentlyFixed | flag2);
    float x = pos.x + fingerDistance.x * num;
    float y2 = pos.y + fingerDistance.y * num;
    //Vector3 b = prevPos[i + 1];
    //Vector3 b2 = prevPos[i - xmax];
    //Vector3 b3 = prevPos[i - 1];
    //Vector3 b4 = prevPos[i + xmax];
    //Vector3 b5 = prevPos[i + 1 + xmax];
    //Vector3 b6 = prevPos[i - 1 - xmax];
    Vector3 a = new Vector3(x, y2, pos.z);
    //float num3 = (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
    //float num4 = (a.x - b3.x) * (a.x - b3.x) + (a.y - b3.y) * (a.y - b3.y);
    //float num5 = (a.x - b4.x) * (a.x - b4.x) + (a.y - b4.y) * (a.y - b4.y);
    //float num6 = (a.x - b2.x) * (a.x - b2.x) + (a.y - b2.y) * (a.y - b2.y);
    //if (num3 > xSize * xSize * relativeMovementConstraint * relativeMovementConstraint || num4 > xSize * xSize * relativeMovementConstraint * relativeMovementConstraint || num5 > ySize * ySize * relativeMovementConstraint * relativeMovementConstraint || num6 > ySize * ySize * relativeMovementConstraint * relativeMovementConstraint) {
    //    a = pos;
    //}
    //float z = pos.z;
    //z = ((!(fingerPressStrength > 0f)) ? Mathf.Max(3.4f * fingerPressStrength * (num * 0.5f + 0.5f), pos.z + num * fingerPressStrength) : Mathf.Min(3.4f * fingerPressStrength * (num * 0.5f + 0.5f), pos.z + num * fingerPressStrength));
    //z = (a.z = (6f * Mathf.Lerp(z * 1f, pos.z, 0.7f) + b.z + b3.z + b4.z + b2.z) / 10f);
    //if (flipProtectionLevel != 0) {
    //    if (flipProtectionLevel >= FlipProtectionLevel.Low && flipProtectionLevel <= FlipProtectionLevel.Med) {
    //        a = HandleTriangleFlip(ref pos, ref a, ref b, ref b2);
    //        a = HandleTriangleFlip(ref pos, ref a, ref b2, ref b6);
    //        a = HandleTriangleFlip(ref pos, ref a, ref b6, ref b3);
    //        a = HandleTriangleFlip(ref pos, ref a, ref b3, ref b4);
    //        a = HandleTriangleFlip(ref pos, ref a, ref b4, ref b5);
    //        a = HandleTriangleFlip(ref pos, ref a, ref b5, ref b);
    //    }
    //    if (flipProtectionLevel >= FlipProtectionLevel.Med && (IsFlip(b - a, b2 - a) || IsFlip(b2 - a, b6 - a) || IsFlip(b6 - a, b3 - a) || IsFlip(b3 - a, b4 - a) || IsFlip(b4 - a, b5 - a) || IsFlip(b5 - a, b - a))) {
    //        a = pos;
    //    }
    //}
    newPos[i] = a;
}


/*
private void CalculateFingerTouch(LeanFinger finger, int i, ref Vector3 pos, ref Vector3 fingerPos, ref Vector3 fingerDistance) {
     cft_num1 = ((pos.x - fingerPos.x) * (pos.x - fingerPos.x) + (pos.y - fingerPos.y) * (pos.y - fingerPos.y)) / (fingerRadius * fingerRadius * 1.4f * 1.4f);
         if (cft_num1 > 0.2f) {
             newPos[i] = prevPos[i];
             return;
         }
         if (cft_num1 < 0.2f) {
             isVertexUnderFinger[i] = true;
         }

         cft_num1 = Mathf.Sqrt(cft_num1) * 1.4f;
         cft_num1 = 1f / (1f + Mathf.Exp(-6f * ((fingerStrength + 1f) * (1f - cft_num1) - 0.5f)));
         cft_flag1 = cft_num1 > 0.1f; //0.25f

         cft_flag2 = isFixedDuringSwipe && cft_flag1;


    if (finger.Up) {
        newPos[i] = prevPos[i];
        if (cft_flag1) {
            cft_num2 = materialStickiness;
            float y = 0f;
            if (newPos[i].z > 0f) {
                if (pokeEffect > 0.01f && cft_num1 > 0.7f) {
                    y = pokeEffect * newPos[i].z * 0.05f;
                    cft_num2 -= pokeEffect;
                }
                cft_num2 += cft_num2 * newPos[i].z * 0.15f;
            }
            vecData[i].velocity.y += y;
            vecData[i].velocity.z += -2f * currentPhysicsSpeed * cft_num2;
        }
        return;
    }


    vecData[i].isCurrentlyFixed = (vecData[i].isPermanentlyFixed | cft_flag2);
    cft_x = pos.x + fingerDistance.x * cft_num1;
    cft_y2 = pos.y + fingerDistance.y * cft_num1;
    cft_b1 = prevPos[i + 1];
    cft_b2 = prevPos[i - xmax];
    cft_b3 = prevPos[i - 1];
    cft_b4 = prevPos[i + xmax];
    cft_b5 = prevPos[i + 1 + xmax];
    cft_b6 = prevPos[i - 1 - xmax];
    cft_a.x = cft_x;
    cft_a.y = cft_y2;
    cft_a.z = pos.z;
    cft_num3 = (cft_a.x - cft_b1.x) * (cft_a.x - cft_b1.x) + (cft_a.y - cft_b1.y) * (cft_a.y - cft_b1.y);
    cft_num4 = (cft_a.x - cft_b3.x) * (cft_a.x - cft_b3.x) + (cft_a.y - cft_b3.y) * (cft_a.y - cft_b3.y);
    cft_num5 = (cft_a.x - cft_b4.x) * (cft_a.x - cft_b4.x) + (cft_a.y - cft_b4.y) * (cft_a.y - cft_b4.y);
    cft_num6 = (cft_a.x - cft_b2.x) * (cft_a.x - cft_b2.x) + (cft_a.y - cft_b2.y) * (cft_a.y - cft_b2.y);
    if (cft_num3 > xSizeSQR_Multiply_relativeMovementConstraintSQR || cft_num4 > xSizeSQR_Multiply_relativeMovementConstraintSQR || cft_num5 > ySizeSQR_Multiply_relativeMovementConstraintSQR || cft_num6 > ySizeSQR_Multiply_relativeMovementConstraintSQR) {
        cft_a = pos;
    }
    cft_z = pos.z;
    cft_z = ((!(fingerPressStrength > 0f)) ? Mathf.Max(3.4f * fingerPressStrength * (cft_num1 * 0.5f + 0.5f), pos.z + cft_num1 * fingerPressStrength) : Mathf.Min(3.4f * fingerPressStrength * (cft_num1 * 0.5f + 0.5f), pos.z + cft_num1 * fingerPressStrength));
    cft_z = (cft_a.z = (6f * Mathf.Lerp(cft_z * 1f, pos.z, 0.7f) + cft_b1.z + cft_b3.z + cft_b4.z + cft_b2.z) / 10f);


    //if (flipProtectionLevel >= FlipProtectionLevel.Low && flipProtectionLevel <= FlipProtectionLevel.Med) {
    //    cft_a = HandleTriangleFlip(ref pos, ref cft_a, ref cft_b1, ref cft_b2);
    //    cft_a = HandleTriangleFlip(ref pos, ref cft_a, ref cft_b2, ref cft_b6);
    //    cft_a = HandleTriangleFlip(ref pos, ref cft_a, ref cft_b6, ref cft_b3);
    //    cft_a = HandleTriangleFlip(ref pos, ref cft_a, ref cft_b3, ref cft_b4);
    //    cft_a = HandleTriangleFlip(ref pos, ref cft_a, ref cft_b4, ref cft_b5);
    //    cft_a = HandleTriangleFlip(ref pos, ref cft_a, ref cft_b5, ref cft_b1);
    //}

    if (flipProtectionLevel >= FlipProtectionLevel.Med &&
        ((cft_b1.x - cft_a.x) * (cft_b2.y - cft_a.y) - (cft_b1.y - cft_a.y) * (cft_b2.x - cft_a.x) > 0f) ||
        ((cft_b2.x - cft_a.x) * (cft_b6.y - cft_a.y) - (cft_b2.y - cft_a.y) * (cft_b6.x - cft_a.x) > 0f) ||
        ((cft_b6.x - cft_a.x) * (cft_b3.y - cft_a.y) - (cft_b6.y - cft_a.y) * (cft_b3.x - cft_a.x) > 0f) ||
        ((cft_b3.x - cft_a.x) * (cft_b4.y - cft_a.y) - (cft_b3.y - cft_a.y) * (cft_b4.x - cft_a.x) > 0f) ||
        ((cft_b4.x - cft_a.x) * (cft_b5.y - cft_a.y) - (cft_b4.y - cft_a.y) * (cft_b5.x - cft_a.x) > 0f) ||
        ((cft_b5.x - cft_a.x) * (cft_b1.y - cft_a.y) - (cft_b5.y - cft_a.y) * (cft_b1.x - cft_a.x) > 0f)
        ) {
        cft_a = pos;
    }
    newPos[i] = cft_a;
}
*/