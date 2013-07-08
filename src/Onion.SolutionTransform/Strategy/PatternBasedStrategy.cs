﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Onion.SolutionTransform.Replacement;

namespace Onion.SolutionTransform.Strategy
{
    abstract public class PatternBasedStrategy : SolutionTransformStrategyBase
    {
        public override async Task TransformAsync()
        {
            await DoTransformAsync(GetFiles(), GetPatternSet());
        }

        public override void Transform()
        {
            var patterns = GetPatternSet();
            var enumerable = patterns as IPattern[] ?? patterns.ToArray();
            if (!enumerable.Any()) return;
            Transform(GetFiles(), enumerable);
        }

        protected void Transform(List<string> files, IEnumerable<IPattern> patterns)
        {
            files.ForEach(f =>
            {
                string contents;
                using (var reader = new StreamReader(f))
                {
                    contents = reader.ReadToEnd();
                }
                contents = patterns.Aggregate(contents, (current, pattern) => pattern.ReplaceInString(current));
                File.WriteAllText(f, contents);
            });
        }

        protected IEnumerable<IPattern> GetPatternSet()
        {
            var patterns = new HashSet<IPattern>();
            var transformableProjects = TransformableProjects(p => p.NameIsModified);
            if (!transformableProjects.Any()) return patterns;
            transformableProjects.ForEach(p => AddProjectPatterns(p, patterns));
            return patterns;
        }

        protected async Task DoTransformAsync(List<string> files, IEnumerable<IPattern> patterns)
        {
            var enumerable = patterns as IPattern[] ?? patterns.ToArray();
            if (!enumerable.Any()) new CancellationTokenSource().Cancel();
            await Task.Run(() => Transform(files, enumerable));
        }

        protected abstract void AddProjectPatterns(TransformableProject p, ISet<IPattern> patterns);
    }
}
