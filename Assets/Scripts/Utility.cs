using System;

public static class Utility
{
    public static T[] ShuffleArray<T>(T[] array, int seed){
        Random prng = new Random(seed);

        for(int i = 0; i < array.Length - 1; i++){
            int randomIndex = prng.Next(i, array.Length - 1);
            (array[randomIndex], array[i]) = (array[i], array[randomIndex]);
        }

        return array;
    }

    public static T[] ShuffleArray<T>(T[] array){

        for(int i = 0; i < array.Length - 1; i++){
            int randomIndex = UnityEngine.Random.Range(i, array.Length);
            (array[randomIndex], array[i]) = (array[i], array[randomIndex]);
        }

        return array;
    }
}
