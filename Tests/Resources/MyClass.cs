using System;
using System.Collections.Generic;

namespace Jora {

    namespace Jira {
        
        public class MyClass {

            public void FirstMethod() {
                Console.WriteLine("First method");
            }

            public void SecondMethod() {
                Console.WriteLine("Second method");
            }

            public void ThirdMethod(int a) {
                Console.WriteLine("Third method (int)");
            }

            public void ThirdMethod(double a) {
                Console.WriteLine("Third method (double)");
            }

            private void ThirdMethod(string a) {
                Console.WriteLine(a);
            }
        }
    }
}