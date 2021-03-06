﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Sequences
{
    public static partial class Sequence
    {
        /// <summary>
        /// Creates a sequence from a given enumerable.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <param name="enumerable">The enumerable to be evaluated.</param>
        /// <returns>A sequence created by lazily-evaluating <paramref name="enumerable"/>.</returns>
        [Pure]
        public static ISequence<T> AsSequence<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            return With(enumerable);
        }

        /// <summary>
        /// Concatenates all sequences in the <paramref name="source"/> collection into a single flattened sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the resulting sequence.</typeparam>
        /// <param name="source">The collection to be flattened.</param>
        /// <returns>A flattened sequence obtained by concatenating all sequences in the <paramref name="source"/> collection.</returns>
        [Pure]
        public static ISequence<TSource> Flatten<TSource>(this IEnumerable<ISequence<TSource>> source)
        {
            return source.SelectMany(seq => seq).AsSequence();
        }

        /// <summary>
        /// Concatenates all collections in the <paramref name="source"/> sequence into a single flattened sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the resulting sequence.</typeparam>
        /// <param name="source">The sequence to be flattened.</param>
        /// <returns>A flattened sequence obtained by concatenating all collections in the <paramref name="source"/> sequence.</returns>
        [Pure]
        public static ISequence<TSource> Flatten<TSource>(this ISequence<IEnumerable<TSource>> source)
        {
            return source.SelectMany(seq => seq);
        }

        /// <summary>
        /// Bypasses a specified number of elements in a sequence and then returns the remaining elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">A sequence to return elements from.</param>
        /// <param name="count">The number of elements to skip before returning the remaining elements.</param>
        /// <returns>A sequence that contains the elements that occur after the specified index in the input sequence.</returns>
        [Pure]
        public static ISequence<TSource> Skip<TSource>(this ISequence<TSource> source, int count)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            ISequence<TSource> seq = source;

            while (seq.NonEmpty && count-- > 0)
                seq = seq.Tail;

            return seq;
        }

        /// <summary>
        /// Bypasses elements in a sequence as long as a specified condition is true and then returns the remaining elements.
        /// If the source sequence represents an infinite set or series and all elements satisfy the given condition, this will never return!
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">A sequence to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A sequence that contains the elements from the input sequence starting at the first element in the linear series that does not pass the test specified by <paramref name="predicate"/></returns>
        [Pure]
        public static ISequence<TSource> SkipWhile<TSource>(this ISequence<TSource> source,
                                                            Func<TSource, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (predicate == null)
                throw new ArgumentNullException("predicate");

            ISequence<TSource> seq = source;

            while (seq.NonEmpty && predicate(seq.Head))
                seq = seq.Tail;

            return seq;
        }

        /// <summary>
        /// Bypasses elements in a sequence as long as a specified condition is true and then returns the remaining elements.
        /// The element's index is used in the logic of the predicate function.
        /// If the source sequence represents an infinite set or series and all elements satisfy the given condition, this will never return!
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The sequence to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition; the second parameter of the function represents the index of the element.</param>
        /// <returns>A sequence that contains the elements from the input sequence starting at the first element in the linear series that does not pass the test specified by <paramref name="predicate"/></returns>
        [Pure]
        public static ISequence<TSource> SkipWhile<TSource>(this ISequence<TSource> source,
                                                            Func<TSource, int, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (predicate == null)
                throw new ArgumentNullException("predicate");

            ISequence<TSource> seq = source;
            int index = 0;

            while (seq.NonEmpty && predicate(seq.Head, index++))
                seq = seq.Tail;

            return seq;
        }

        /// <summary>
        /// Projects each element of a sequence into a new sequence.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements returned by <paramref name="selector"/>.</typeparam>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>A sequence whose elements are the result of invoking the transform function on each element of <paramref name="source"/>.</returns>
        [Pure]
        public static ISequence<TResult> Select<TSource, TResult>(this ISequence<TSource> source,
                                                                  Func<TSource, TResult> selector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");

            return source.IsEmpty
                       ? Empty<TResult>()
                       : new Sequence<TResult>(selector(source.Head),
                                               () => source.Tail.Select(selector));
        }

        /// <summary>
        /// Projects each element of a sequence into a new sequence.
        /// Each element's index is used in the logic of the selector function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements returned by <paramref name="selector"/>.</typeparam>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element and its index.</param>
        /// <returns>A sequence whose elements are the result of invoking the transform function on each element of <paramref name="source"/>.</returns>
        [Pure]
        public static ISequence<TResult> Select<TSource, TResult>(this ISequence<TSource> source,
                                                                  Func<TSource, int, TResult> selector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            return Select(source, selector, 0);
        }

        private static ISequence<TResult> Select<TSource, TResult>(ISequence<TSource> source,
                                                                   Func<TSource, int, TResult> selector, int index)
        {
            return source.IsEmpty
                       ? Empty<TResult>()
                       : new Sequence<TResult>(selector(source.Head, index),
                                               () => Select(source.Tail, selector, index + 1));
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="IEnumerable{T}"/> and flattens the resulting enumerables into one sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the sequence returned by selector.</typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>A sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        [Pure]
        public static ISequence<TResult> SelectMany<TSource, TResult>(this ISequence<TSource> source,
                                                                      Func<TSource, IEnumerable<TResult>> selector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");

            //apply "selector" to the source's head, and iterate through the resulting enumerable
            return source.IsEmpty
                       ? Empty<TResult>()
                       : SelectMany(source, selector, selector(source.Head).GetEnumerator());
        }

        private static ISequence<TResult> SelectMany<TSource, TResult>(ISequence<TSource> source,
                                                                       Func<TSource, IEnumerable<TResult>> selector,
                                                                       IEnumerator<TResult> iter)
        {
            if (iter.TryMoveNext())
                return new Sequence<TResult>(iter.Current,
                                             () => SelectMany(source, selector, iter));

            //if this iterator has been exhausted, move onto the tail
            return source.Tail.SelectMany(selector);
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="IEnumerable{T}"/> and flattens the resulting enumerables into one sequence.
        /// The index of each source element is used in the projected form of that element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the sequence returned by selector.</typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the element.</param>
        /// <returns>A sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        [Pure]
        public static ISequence<TResult> SelectMany<TSource, TResult>(this ISequence<TSource> source,
                                                                      Func<TSource, int, IEnumerable<TResult>> selector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");

            return SelectMany(source, selector, 0);
        }

        private static ISequence<TResult> SelectMany<TSource, TResult>(ISequence<TSource> source,
                                                                       Func<TSource, int, IEnumerable<TResult>> selector,
                                                                       int index)
        {
            //apply "selector" to the source's head, and iterate through the resulting enumerable
            return source.IsEmpty
                       ? Empty<TResult>()
                       : SelectMany(source, selector, selector(source.Head, index).GetEnumerator(), index);
        }

        private static ISequence<TResult> SelectMany<TSource, TResult>(ISequence<TSource> source,
                                                                       Func<TSource, int, IEnumerable<TResult>> selector,
                                                                       IEnumerator<TResult> iter, int index)
        {
            if (iter.TryMoveNext())
                return new Sequence<TResult>(iter.Current,
                                             () => SelectMany(source, selector, iter, index));

            //if this iterator has been exhausted, move onto the tail
            return SelectMany(source.Tail, selector, index + 1);
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="IEnumerable{T}"/>, flattens the resulting enumerables into one sequence, and invokes a result selector function on each element therein.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TCollection">The type of the intermediate elements collected by <paramref name="collectionSelector"/>.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element of the input sequence.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>A sequence whose elements are the result of invoking the one-to-many transform function <paramref name="collectionSelector"/> on each element of <paramref name="source"/> and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        [Pure]
        public static ISequence<TResult> SelectMany<TSource, TCollection, TResult>(
            this ISequence<TSource> source,
            Func<TSource, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (collectionSelector == null) throw new ArgumentNullException("collectionSelector");
            if (resultSelector == null) throw new ArgumentNullException("resultSelector");

            //apply "selector" to the source's head, and iterate through the resulting enumerable
            return source.IsEmpty
                       ? Empty<TResult>()
                       : SelectMany(source, collectionSelector, resultSelector,
                                    collectionSelector(source.Head).GetEnumerator());
        }

        private static ISequence<TResult> SelectMany<TSource, TCollection, TResult>(
            ISequence<TSource> source,
            Func<TSource, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector,
            IEnumerator<TCollection> iter)
        {
            if (iter.TryMoveNext())
                return new Sequence<TResult>(resultSelector(source.Head, iter.Current),
                                             () => SelectMany(source, collectionSelector, resultSelector, iter));

            //if this iterator has been exhausted, move onto the tail
            return source.Tail.SelectMany(collectionSelector, resultSelector);
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="IEnumerable{T}"/>, flattens the resulting enumerables into one sequence, and invokes a result selector function on each element therein.
        /// The index of each source element is used in the intermediate projected form of that element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TCollection">The type of the intermediate elements collected by <paramref name="collectionSelector"/>.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each <paramref name="source"/> element; the second parameter of the function represents the index of the <paramref name="source"/> element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>A sequence whose elements are the result of invoking the one-to-many transform function <paramref name="collectionSelector"/> on each element of <paramref name="source"/> and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        [Pure]
        public static ISequence<TResult> SelectMany<TSource, TCollection, TResult>(
            this ISequence<TSource> source,
            Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (collectionSelector == null) throw new ArgumentNullException("collectionSelector");
            if (resultSelector == null) throw new ArgumentNullException("resultSelector");

            return SelectMany(source, collectionSelector, resultSelector, 0);
        }

        private static ISequence<TResult> SelectMany<TSource, TCollection, TResult>(
            ISequence<TSource> source,
            Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector,
            int index)
        {
            //apply "collectionSelector" to the source's head, and iterate through the resulting enumerable
            return source.IsEmpty
                       ? Empty<TResult>()
                       : SelectMany(source, collectionSelector, resultSelector,
                                    collectionSelector(source.Head, index).GetEnumerator(), index);
        }

        private static ISequence<TResult> SelectMany<TSource, TCollection, TResult>(
            ISequence<TSource> source,
            Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector,
            IEnumerator<TCollection> iter,
            int index)
        {
            if (iter.TryMoveNext())
                return new Sequence<TResult>(resultSelector(source.Head, iter.Current),
                                             () => SelectMany(source, collectionSelector, resultSelector, iter, index));

            //if this iterator has been exhausted, move onto the tail
            return SelectMany(source.Tail, collectionSelector, resultSelector, index + 1);
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">A sequence to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A sequence that contains elements from <paramref name="source"/> that satisfy the condition.</returns>
        [Pure]
        public static ISequence<TSource> Where<TSource>(this ISequence<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (predicate == null) throw new ArgumentNullException("predicate");

            while (source.NonEmpty && !predicate(source.Head))
                source = source.Tail;

            return source.IsEmpty
                       ? source
                       : new Sequence<TSource>(source.Head, () => source.Tail.Where(predicate));
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// Each element's index is used in the logic of the predicate function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">A sequence to filter.</param>
        /// <param name="predicate">A function to test each element and its index for a condition.</param>
        /// <returns>A sequence that contains elements from <paramref name="source"/> that satisfy the condition.</returns>
        [Pure]
        public static ISequence<TSource> Where<TSource>(this ISequence<TSource> source,
                                                        Func<TSource, int, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (predicate == null) throw new ArgumentNullException("predicate");

            return Where(source, predicate, 0);
        }

        private static ISequence<TSource> Where<TSource>(ISequence<TSource> source,
                                                         Func<TSource, int, bool> predicate,
                                                         int index)
        {
            while (source.NonEmpty && !predicate(source.Head, index++))
                source = source.Tail;

            return source.IsEmpty
                       ? source
                       : new Sequence<TSource>(source.Head, () => Where(source.Tail, predicate, index));
        }

        /// <summary>
        /// Returns a specified number of contiguous elements from the start of a sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The sequence to return elements from.</param>
        /// <param name="count">The number of elements to return.</param>
        /// <returns>A sequence that contains the specified number of elements from the start of the input sequence.</returns>
        [Pure]
        public static ISequence<TSource> Take<TSource>(this ISequence<TSource> source, int count)
        {
            if (source == null) throw new ArgumentNullException("source");

            return count <= 0 || source.IsEmpty
                       ? Empty<TSource>()
                       : new Sequence<TSource>(source.Head, () => source.Tail.Take(count - 1));
        }

        /// <summary>
        /// Returns elements from a sequence as long as a specified condition is true.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The sequence to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A sequence that contains the elements from the input sequence that occur before the element at which the test no longer passes.</returns>
        [Pure]
        public static ISequence<TSource> TakeWhile<TSource>(this ISequence<TSource> source,
                                                            Func<TSource, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (predicate == null) throw new ArgumentNullException("predicate");

            return source.IsEmpty || !predicate(source.Head)
                       ? Empty<TSource>()
                       : new Sequence<TSource>(source.Head, () => source.Tail.TakeWhile(predicate));
        }

        /// <summary>
        /// Returns elements from a sequence as long as a specified condition is true.
        /// The element's index is used in the logic of the predicate function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The sequence to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition; the second parameter of the function represents the index of the element.</param>
        /// <returns>A sequence that contains the elements from the input sequence that occur before the element at which the test no longer passes.</returns>
        [Pure]
        public static ISequence<TSource> TakeWhile<TSource>(this ISequence<TSource> source,
                                                            Func<TSource, int, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (predicate == null) throw new ArgumentNullException("predicate");
            return TakeWhile(source, predicate, 0);
        }

        private static ISequence<TSource> TakeWhile<TSource>(ISequence<TSource> source,
                                                             Func<TSource, int, bool> predicate,
                                                             int index)
        {
            return source.IsEmpty || !predicate(source.Head, index)
                       ? Empty<TSource>()
                       : new Sequence<TSource>(source.Head, () => TakeWhile(source.Tail, predicate, index + 1));
        }

        /// <summary>
        /// Applies a specified function to the corresponding elements of two sequences, producing a sequence of the results.
        /// </summary>
        /// <typeparam name="TFirst">The type of the elements of <paramref name="first"/>.</typeparam>
        /// <typeparam name="TSecond">The type of the elements of <paramref name="second"/>.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the result sequence.</typeparam>
        /// <param name="first">The first sequence to merge.</param>
        /// <param name="second">The second sequence to merge.</param>
        /// <param name="resultSelector">A function that specifies how to merge the elements from the two sequences.</param>
        /// <returns>A sequence that contains merged elements of two input sequences.</returns>
        [Pure]
        public static ISequence<TResult> Zip<TFirst, TSecond, TResult>(this ISequence<TFirst> first,
                                                                       IEnumerable<TSecond> second,
                                                                       Func<TFirst, TSecond, TResult> resultSelector)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (second == null) throw new ArgumentNullException("second");
            if (resultSelector == null) throw new ArgumentNullException("resultSelector");
            return Zip(first, second.GetEnumerator(), resultSelector);
        }

        private static ISequence<TResult> Zip<TFirst, TSecond, TResult>(ISequence<TFirst> first,
                                                                        IEnumerator<TSecond> second,
                                                                        Func<TFirst, TSecond, TResult> resultSelector)
        {
            //when either sequence is empty, return an empty sequence.
            return first.NonEmpty && second.TryMoveNext()
                       ? new Sequence<TResult>(resultSelector(first.Head, second.Current),
                                               () => Zip(first.Tail, second, resultSelector))
                       : Empty<TResult>();
        }

        /// <summary>
        /// Returns distinct elements from a sequence by using the default equality comparer to compare values.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The sequence to remove duplicate elements from.</param>
        /// <returns>A sequence that contains distinct elements from the input sequence.</returns>
        [Pure]
        public static ISequence<TSource> Distinct<TSource>(this ISequence<TSource> source)
        {
            return source.Distinct(EqualityComparer<TSource>.Default);
        }

        /// <summary>
        /// Returns distinct elements from a sequence by using a specified <see cref="IEqualityComparer{T}"/> to compare values.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The sequence to remove duplicate elements from.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to compare elements.</param>
        /// <returns>A sequence that contains distinct elements from the input sequence.</returns>
        [Pure]
        public static ISequence<TSource> Distinct<TSource>(this ISequence<TSource> source,
                                                           IEqualityComparer<TSource> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (comparer == null) throw new ArgumentNullException("comparer");
            return Except(source, new HashSet<TSource>(comparer));
        }

        /// <summary>
        /// Produces the set difference of two sequences by using the default equality comparer to compare values.
        /// If the second sequence represents an infinite set or series, this will never return!
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <param name="first">A sequence whose elements that are not also in <paramref name="second"/> will be returned.</param>
        /// <param name="second">A sequence whose elements that also occur in this sequence will cause those elements to be removed from the returned sequence.</param>
        /// <returns>A sequence that contains the set difference of the elements of two sequences.</returns>
        [Pure]
        public static ISequence<TSource> Except<TSource>(this ISequence<TSource> first, IEnumerable<TSource> second)
        {
            return first.Except(second, EqualityComparer<TSource>.Default);
        }

        /// <summary>
        /// Produces the set difference of two sequences by using the specified <see cref="IEqualityComparer{T}"/> to compare values.
        /// If the second sequence represents an infinite set or series, this will never return!
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <param name="first">A sequence whose elements that are not also in <paramref name="second"/> will be returned.</param>
        /// <param name="second">A sequence whose elements that also occur in this sequence will cause those elements to be removed from the returned sequence.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to compare values.</param>
        /// <returns>A sequence that contains the set difference of the elements of two sequences.</returns>
        [Pure]
        public static ISequence<TSource> Except<TSource>(this ISequence<TSource> first, IEnumerable<TSource> second,
                                                         IEqualityComparer<TSource> comparer)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (second == null) throw new ArgumentNullException("second");
            if (comparer == null) throw new ArgumentNullException("comparer");
            return Except(first, new HashSet<TSource>(second, comparer));
        }

        private static ISequence<TSource> Except<TSource>(ISequence<TSource> source, HashSet<TSource> bucket)
        {
            while (source.NonEmpty && !bucket.Add(source.Head))
                source = source.Tail;

            return source.IsEmpty
                       ? Empty<TSource>()
                       : new Sequence<TSource>(source.Head, () => Except(source.Tail, bucket));
        }

        /// <summary>
        /// Produces the set intersection of two sequences by using the default equality comparer to compare values.
        /// If the second sequence represents an infinite set or series, this will never return!
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <param name="first">A sequence whose distinct elements that also appear in <paramref name="second"/> will be returned.</param>
        /// <param name="second">A sequence whose distinct elements that also appear in the first sequence will be returned.</param>
        /// <returns>A sequence that contains the elements that form the set intersection of two sequences.</returns>
        [Pure]
        public static ISequence<TSource> Intersect<TSource>(this ISequence<TSource> first, IEnumerable<TSource> second)
        {
            return first.Intersect(second, EqualityComparer<TSource>.Default);
        }

        /// <summary>
        /// Produces the set intersection of two sequences by using the specified <see cref="IEqualityComparer{T}"/> to compare values.
        /// If the second sequence represents an infinite set or series, this will never return!
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <param name="first">A sequence whose distinct elements that also appear in <paramref name="second"/> will be returned.</param>
        /// <param name="second">A sequence whose distinct elements that also appear in the first sequence will be returned.</param>
        /// <param name="comparer"></param>
        /// <returns>A sequence that contains the elements that form the set intersection of two sequences.</returns>
        [Pure]
        public static ISequence<TSource> Intersect<TSource>(this ISequence<TSource> first, IEnumerable<TSource> second,
                                                            IEqualityComparer<TSource> comparer)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (second == null) throw new ArgumentNullException("second");
            if (comparer == null) throw new ArgumentNullException("comparer");
            return Intersect(first, new HashSet<TSource>(second, comparer));
        }

        private static ISequence<TSource> Intersect<TSource>(ISequence<TSource> source, HashSet<TSource> bucket)
        {
            while (source.NonEmpty && !bucket.Remove(source.Head))
                source = source.Tail;

            return source.IsEmpty
                       ? Empty<TSource>()
                       : new Sequence<TSource>(source.Head, () => Intersect(source.Tail, bucket));
        }

        /// <summary>
        /// Inverts the order of the elements in a sequence.
        /// If the source sequence represents an infinite set or series, this will never return!
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">A sequence of values to reverse.</param>
        /// <returns>A sequence whose elements correspond to those of the input sequence in reverse order.</returns>
        [Pure]
        public static ISequence<TSource> Reverse<TSource>(this ISequence<TSource> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return new Stack<TSource>(source).AsSequence();
        }

        /// <summary>
        /// Computes the sum of the sequence of <see cref="BigInteger"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// If the source sequence represents an infinite set or series, this will never return!
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector"></param>
        /// <returns>A transform function to apply to each element.</returns>
        [Pure]
        public static BigInteger Sum<TSource>(this ISequence<TSource> source, Func<TSource, BigInteger> selector)
        {
            return source.Select(selector).Sum();
        }

        /// <summary>
        /// Computes the sum of the sequence of <see cref="Nullable{BigInteger}"/> values that are obtained by invoking a transform function on each element of the input sequence.
        /// If the source sequence represents an infinite set or series, this will never return!
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector"></param>
        /// <returns>A transform function to apply to each element.</returns>
        [Pure]
        public static BigInteger? Sum<TSource>(this ISequence<TSource> source, Func<TSource, BigInteger?> selector)
        {
            return source.Select(selector).Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="BigInteger"/> values.
        /// If the source sequence represents an infinite set or series, this will never return!
        /// </summary>
        /// <param name="source">A sequence of <see cref="BigInteger"/> values to calculate the sum of.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        [Pure]
        public static BigInteger Sum(this ISequence<BigInteger> source)
        {
            return source.Aggregate<BigInteger, BigInteger>(
                0,
                (acc, i) => acc + i);
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Nullable{BigInteger}"/> values.
        /// If the source sequence represents an infinite set or series, this will never return!
        /// </summary>
        /// <param name="source">A sequence of <see cref="Nullable{BigInteger}"/> values to calculate the sum of.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        [Pure]
        public static BigInteger? Sum(this ISequence<BigInteger?> source)
        {
            return source.Aggregate<BigInteger?, BigInteger?>(
                0,
                (acc, i) => i == null ? acc : acc + i);
        }

        /// <summary>
        /// Produces the set union of two sequences by using the default equality comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <param name="first">A sequence whose distinct elements form the first set for the union.</param>
        /// <param name="second">A sequence whose distinct elements form the second set for the union.</param>
        /// <returns></returns>
        [Pure]
        public static ISequence<TSource> Union<TSource>(this ISequence<TSource> first, IEnumerable<TSource> second)
        {
            return first.Union(second, EqualityComparer<TSource>.Default);
        }

        /// <summary>
        /// Produces the set union of two sequences by using a specified <see cref="IEqualityComparer{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <param name="first">A sequence whose distinct elements form the first set for the union.</param>
        /// <param name="second">A sequence whose distinct elements form the second set for the union.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to compare values.</param>
        /// <returns></returns>
        [Pure]
        public static ISequence<TSource> Union<TSource>(this ISequence<TSource> first, IEnumerable<TSource> second,
                                                        IEqualityComparer<TSource> comparer)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (second == null) throw new ArgumentNullException("second");
            if (comparer == null) throw new ArgumentNullException("comparer");
            return Union(first, second.GetEnumerator(), new HashSet<TSource>(comparer));
        }

        private static ISequence<TSource> Union<TSource>(ISequence<TSource> first, IEnumerator<TSource> second,
                                                         HashSet<TSource> bucket)
        {
            //try to find the next distinct item from "first"
            while (first.NonEmpty && !bucket.Add(first.Head))
                first = first.Tail;

            if (first.NonEmpty)
                return new Sequence<TSource>(first.Head, () => Union(first.Tail, second, bucket));

            //try to find the next distinct item from "second"
            while (second.TryMoveNext())
                if (bucket.Add(second.Current))
                    return new Sequence<TSource>(second.Current, () => Union(first, second, bucket));

            return Empty<TSource>();
        }
    }
}
