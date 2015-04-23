using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Algorithms;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class FractionTest
    {
        [Test]
        public void Basic()
        {
            Fraction a = new Fraction(4, 6);
            Assert.AreEqual(2, a.Numerator);
            Assert.AreEqual(3, a.Denominator);
            Assert.IsTrue(a.Equals(new Fraction(2, 3)));

            a = a + (Fraction)2;
            Assert.AreEqual(8, a.Numerator);
            Assert.AreEqual(3, a.Denominator);
            Assert.IsTrue(a.Equals(new Fraction(8, 3)));
            a = a + new Fraction(1, 3);
            Assert.AreEqual(3, a.Numerator);
            Assert.AreEqual(1, a.Denominator);
            Assert.IsTrue(a.Equals((Fraction)3));

            Fraction b = new Fraction(3, -4);
            Assert.AreEqual(-3, b.Numerator);
            Assert.AreEqual(4, b.Denominator);
            Assert.IsTrue(b.Equals(new Fraction(-3, 4)));

            Fraction c = new Fraction(0, -25353);
            Assert.AreEqual(0, c.Numerator);
            Assert.AreEqual(1, c.Denominator);
            Assert.IsTrue(c.Equals((Fraction)0));

            Assert.IsTrue(a > b);
            Assert.IsTrue(b <= a);
            Assert.IsTrue(a > c);
            Assert.IsTrue(c <= a);
            Assert.IsTrue(c > b);
            Assert.IsTrue(b <= c);
            Fraction d = b;
            Assert.IsTrue(d == b);
            Assert.IsFalse(d != b);
            Assert.IsTrue(d >= b);
            Assert.IsTrue(d <= b);
            Assert.IsFalse(d > b);
            Assert.IsFalse(d < b);

        }
    }
}
