// C# code​​​​​​‌​​‌​‌‌​‌​‌‌​​​​‌​​‌‌​‌‌​ below
using System;

public class Algorithm
{
     // Newton-Raphson
    // X^n - a = 0 ==> X^n = a
    // x(k+1) = x(k) - f(x(k)/f'(x(k))
    // f(x) = x^n -a ==> f'(x) = n x^(n-1)
    // then x(k+1) = x(k) - ((xk^n -a)/n x^n -1)
    // x(k)(n x^n -1)/(n x^n-1) - (xk^n -a)/n x^n -1)
    // (n-1)x^n+a / n x^n-1
    // ((n-1) x + a/x^(n-1))/n


    public static double Power(double value, int expo){
        double res = 1;
        for(int i = 0; i < expo; i++){
            res*=value;
        }
        return res;
    }

    public static double AbsValue(double val){
        return val < 0 ? -val : val;
    }
    
    /// Calculates the nth root of a number
    public static double GetNthRoot(double number, int n)
    {
        if(number == 0)
            return 0;
        if(number == 1)
            return 1;

        double guess = number /n;

        // Precision System
        // Used to check whether our estimations is less than our newguess
        double epsilon =1e-10; 

        while(true){
            double newGuess = ((n-1)*guess + number / Power(guess,n-1))/n;
            
            if(AbsValue(newGuess - guess) < epsilon)
                break;    

            guess = newGuess;
        }
        return guess;
    }
}
