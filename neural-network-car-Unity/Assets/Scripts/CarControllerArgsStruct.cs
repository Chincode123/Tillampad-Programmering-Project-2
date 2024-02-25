// De h�r parametrarna (orkar inte �ndra p� namnet av structen) beh�vde passeras flera g�nger till samma st�lle s� jag satte
// ihop dem till en struct s� de inte skulle beh�va skrivas ut s� m�nga g�nger

[System.Serializable] public struct CarControllerArgs{
    public float acceleration;
    public float maxSpeed;
    public float rotationSpeed;
    public float deceleration;
    public float maxRotationRadius;
}