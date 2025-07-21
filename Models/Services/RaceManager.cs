using System;
using System.Collections.Generic;
using System.Linq;
using CSharpSpeedRush.Models;

namespace CSharpSpeedRush.Services
{
    public class RaceManager
    {
        public List<Car> Cars { get; private set; }
        public Dictionary<Car, Track> CarTracks { get; private set; }
        public Car PlayerCar { get; private set; }
        public double TimeRemaining { get; private set; } = 120; // seconds
        public bool RaceFinished { get; private set; } = false;
        public Car Winner { get; private set; }

        private Random _rand = new Random();

        public RaceManager(Car playerCar, List<Car> aiCars)
        {
            Cars = new List<Car> { playerCar };
            Cars.AddRange(aiCars);
            PlayerCar = playerCar;
            CarTracks = Cars.ToDictionary(car => car, car => new Track());
        }

        // Player action, AI actions are chosen automatically
        public void AdvanceRace(RaceAction playerAction)
        {
            // Player car
            PerformAction(PlayerCar, playerAction);

            // AI cars
            foreach (var car in Cars)
            {
                if (car == PlayerCar) continue;
                var aiAction = GetAIAction(car);
                PerformAction(car, aiAction);
            }

            TimeRemaining -= 7; // Each turn is 7 seconds
            CheckRaceFinished();
        }

        private void PerformAction(Car car, RaceAction action)
        {
            var track = CarTracks[car];
            switch (action)
            {
                case RaceAction.SpeedUp:
                    car.ConsumeFuel(1.5);
                    track.Advance(30);
                    break;
                case RaceAction.Maintain:
                    car.ConsumeFuel(1.0);
                    track.Advance(20);
                    break;
                case RaceAction.PitStop:
                    car.Refuel();
                    break;
            }
        }

        private RaceAction GetAIAction(Car car)
        {
            // Simple AI: if fuel is low, pit stop; else random between maintain and speed up
            if (car.Fuel < car.MaxFuel * 0.2)
                return RaceAction.PitStop;
            return _rand.NextDouble() < 0.5 ? RaceAction.Maintain : RaceAction.SpeedUp;
        }

        private void CheckRaceFinished()
        {
            foreach (var car in Cars)
            {
                if (CarTracks[car].IsRaceFinished)
                {
                    RaceFinished = true;
                    Winner = car;
                    break;
                }
            }
            if (TimeRemaining <= 0)
            {
                RaceFinished = true;
                // Winner is the car with the most progress
                Winner = Cars.OrderByDescending(c => CarTracks[c].CurrentLap)
                              .ThenByDescending(c => CarTracks[c].LapProgress)
                              .First();
            }
        }
    }
}
// CSharpSpeedRush.Models.Services.RaceManager.cs