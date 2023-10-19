using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers.Exotic
{
    /// <summary>
    ///     Describes a constraint in a number-placement puzzle consisting of a set of triplets of cells and a validity
    ///     predicate. The constraint mandates that an exact number of triples must satisfy this predicate.</summary>
    public class TripletValidityConstraint : Constraint
    {
        /// <summary>Specifies the triplets of cells affected by this constraint.</summary>
        public int[][] Triplets { get; private set; }
        /// <summary>Specifies how many of the <see cref="Triplets"/> must satisfy the <see cref="IsValid"/> predicate.</summary>
        public int NumValid { get; private set; }
        /// <summary>Defines the validity predicate.</summary>
        public Func<int, int, int, bool> IsValid { get; private set; }

        /// <summary>
        ///     Construtor.</summary>
        /// <param name="triplets">
        ///     The triplets of cells affected by this constraint.</param>
        /// <param name="numValid">
        ///     Specifies how many of the <paramref name="triplets"/> must satisfy the <paramref name="isValid"/> predicate</param>
        /// <param name="isValid">
        ///     The validity predicate.</param>
        public TripletValidityConstraint(int[][] triplets, int numValid, Func<int, int, int, bool> isValid)
            : base(triplets.SelectMany(x => x).Distinct())
        {
            if (triplets == null)
                throw new ArgumentNullException(nameof(triplets));
            if (triplets.IndexOf(t => t == null || t.Length != 3) is int p && p != -1)
                throw new ArgumentException($"Triplet at index {p} is {(triplets[p] == null ? "null" : "not of length 3")}.", nameof(triplets));

            Triplets = triplets;
            NumValid = numValid;
            IsValid = isValid;
        }

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            HashSet<int> minValid = [], maxValid = [];
            for (var grIx = 0; grIx < Triplets.Length; grIx++)
            {
                var gr = Triplets[grIx];
                bool allValid = true, anyValid = false;
                foreach (var a in state.Possible(gr[0]))
                    foreach (var r in state.Possible(gr[1]))
                        foreach (var b in state.Possible(gr[2]))
                            if (IsValid(a, r, b))
                            {
                                anyValid = true;
                                if (!allValid)
                                    goto done;
                            }
                            else
                            {
                                allValid = false;
                                if (anyValid)
                                    goto done;
                            }
                done:
                if (anyValid) maxValid.Add(grIx);
                if (allValid) minValid.Add(grIx);
            }

            if (minValid.Count > NumValid || maxValid.Count < NumValid)
                return ConstraintResult.Violation;

            var haveMin = minValid.Count == NumValid;
            var haveMax = maxValid.Count == NumValid;

            if (haveMin)
            {
                if (haveMax)
                    // Constraint is fully satisfied
                    return ConstraintResult.Remove;

                // We have the required number of valid triplets ⇒ make sure the other groups are not going to be valid
                var newConstraints = new List<Constraint>();
                for (var grIx = 0; grIx < Triplets.Length; grIx++)
                    if (!minValid.Contains(grIx))
                        newConstraints.Add(new ThreeCellLambdaConstraint(Triplets[grIx][0], Triplets[grIx][1], Triplets[grIx][2], (a, b, c) => !IsValid(a, b, c)));
                return newConstraints;
            }

            if (haveMax)
            {
                // We can only have the required number of valid triplets ⇒ make sure that they are all going to be valid
                var newConstraints = new List<Constraint>();
                for (var grIx = 0; grIx < Triplets.Length; grIx++)
                    if (maxValid.Contains(grIx))
                        newConstraints.Add(new ThreeCellLambdaConstraint(Triplets[grIx][0], Triplets[grIx][1], Triplets[grIx][2], IsValid));
                return newConstraints;
            }

            return null;
        }
    }
}
