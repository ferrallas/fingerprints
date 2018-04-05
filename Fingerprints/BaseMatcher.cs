using System;
using System.Collections.Generic;
using System.Drawing;
using Fingerprints.Computation;
using Fingerprints.Model;

namespace Fingerprints
{
    public abstract class BaseMatcher<TFeature> where TFeature : class
    {
        public abstract TFeature Extract(Bitmap image);

        public abstract double Match(TFeature query, TFeature template, out List<MinutiaPair> matchingMtiae);

        public void Store(IStoreProvider<TFeature> storage, Bitmap bitmap, string subjectId)
        {
            var extract = Extract(ImageProvider.AdaptImage(bitmap));

            storage.Add(new Candidate<TFeature>
            {
                EntryId = subjectId,
                Feautures = extract
            });
        }

        public IEnumerable<Match> Match(IStoreProvider<TFeature> storage, Bitmap bitmap, int skip, int take)
        {
            var extract = Extract(ImageProvider.AdaptImage(bitmap));

            var list = new List<Match>();
            foreach (var candidate in storage.GetCandidates(skip,take))
            {
                var retrieved = storage.Retrieve(candidate);
                var score = Match(extract, retrieved, out var matchingMtiae);

                if (Math.Abs(score) < double.Epsilon || matchingMtiae == null)
                    continue;

                if (matchingMtiae.Count > 10)
                    list.Add(new Match
                    {
                        Confidence = score,
                        EntryId = candidate,
                        MatchingPoints = matchingMtiae.Count
                    });
            }
            return list;
        }
    }
}
