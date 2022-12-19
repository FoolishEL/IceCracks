public class Constraint
{
    public int i;

    public float restDistance;

    public float strength;

    public bool isActive = true;

    public Constraint(int i, float restDistance, float strength) {
        this.i = i;
        this.restDistance = restDistance;
        this.strength = strength;
    }
}
