namespace CSharpSpeedRush.Models
{
    public class Car
    {
        public string Name { get; set; }
        public double MaxFuel { get; set; }
        public double Fuel { get; set; }
        public double Speed { get; set; }
        public double FuelConsumption { get; set; }

        public Car(string name, double maxFuel, double speed, double fuelConsumption)
        {
            Name = name;
            MaxFuel = maxFuel;
            Fuel = maxFuel;
            Speed = speed;
            FuelConsumption = fuelConsumption;
        }

        public void Refuel()
        {
            Fuel = MaxFuel;
        }

        public void ConsumeFuel(double multiplier = 1.0)
        {
            Fuel -= FuelConsumption * multiplier;
            if (Fuel < 0) Fuel = 0;
        }
    }
}
// CSharpSpeedRush.Models.Car.cs