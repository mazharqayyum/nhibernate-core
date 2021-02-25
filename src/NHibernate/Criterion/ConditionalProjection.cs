namespace NHibernate.Criterion
{
	using System;
	using System.Collections.Generic;
	using Engine;
	using SqlCommand;
	using Type;

	[Serializable]
	public class ConditionalProjection : SimpleProjection
	{
		private readonly ICriterion criterion;
		private readonly IProjection whenTrue;
		private readonly IProjection whenFalse;

		public ConditionalProjection(ICriterion criterion, IProjection whenTrue, IProjection whenFalse)
		{
			this.whenTrue = whenTrue;
			this.whenFalse = whenFalse;
			this.criterion = criterion;
		}

		public override bool IsAggregate
		{
			get
			{
				IProjection[] projections = criterion.GetProjections();
				if(projections != null)
				{
					foreach (IProjection projection in projections)
					{
						if (projection.IsAggregate)
							return true;
					}
				}
				if(whenFalse.IsAggregate)
					return true;
				if(whenTrue.IsAggregate)
					return true;
				return false;
			}
		}

		public override SqlString ToSqlString(ICriteria criteria, int position, ICriteriaQuery criteriaQuery)
		{
			SqlString condition = criterion.ToSqlString(criteria, criteriaQuery);
			var ifTrue = CriterionUtil.GetColumnNameAsSqlStringPart(whenTrue, criteriaQuery, criteria);
			var ifFalse = CriterionUtil.GetColumnNameAsSqlStringPart(whenFalse, criteriaQuery, criteria);
			return new SqlString("(case when ", condition, " then ", ifTrue, " else ", ifFalse, " end) as ",
			                     GetColumnAlias(position));
		}

		public override IType[] GetTypes(ICriteria criteria, ICriteriaQuery criteriaQuery)
		{
			IType[] trueTypes = whenTrue.GetTypes(criteria, criteriaQuery);
			IType[] falseTypes = whenFalse.GetTypes(criteria, criteriaQuery);

			bool areEqual = trueTypes.Length == falseTypes.Length;
			if (areEqual)
			{
				for (int i = 0; i < trueTypes.Length; i++)
				{
					if(trueTypes[i].ReturnedClass != falseTypes[i].ReturnedClass)
					{
						areEqual = false;
						break;
					}
				}
			}
			if(areEqual == false)
			{
				string msg = "Both true and false projections must return the same types."+ Environment.NewLine +
				             "But True projection returns: ["+string.Join<IType>(", ", trueTypes) +"] "+ Environment.NewLine+
				             "And False projection returns: ["+string.Join<IType>(", ", falseTypes)+ "]";

				throw new HibernateException(msg);
			}

			return trueTypes;
		}

		public override TypedValue[] GetTypedValues(ICriteria criteria, ICriteriaQuery criteriaQuery)
		{
			List<TypedValue> tv = new List<TypedValue>();
			tv.AddRange(criterion.GetTypedValues(criteria, criteriaQuery));
			tv.AddRange(whenTrue.GetTypedValues(criteria, criteriaQuery));
			tv.AddRange(whenFalse.GetTypedValues(criteria, criteriaQuery));
			return tv.ToArray();
		}

		public override bool IsGrouped
		{
			get
			{
				IProjection[] projections = criterion.GetProjections();
				if(projections != null)
				{
					foreach (IProjection projection in projections)
					{
						if (projection.IsGrouped)
							return true;
					}
				}
				if(whenFalse.IsGrouped)
					return true;
				if(whenTrue.IsGrouped)
					return true;
				return false;
			}
		}

		public override SqlString ToGroupSqlString(ICriteria criteria, ICriteriaQuery criteriaQuery)
		{
			SqlStringBuilder buf = new SqlStringBuilder();
			IProjection[] projections = criterion.GetProjections();
			if(projections != null)
			{
				foreach (IProjection proj in projections)
				{
					if (proj.IsGrouped)
					{
						buf.Add(proj.ToGroupSqlString(criteria, criteriaQuery)).Add(", ");
					}
				}
			}
			if(whenFalse.IsGrouped)
				buf.Add(whenFalse.ToGroupSqlString(criteria, criteriaQuery)).Add(", ");
			if(whenTrue.IsGrouped)
				buf.Add(whenTrue.ToGroupSqlString(criteria, criteriaQuery)).Add(", ");

			if(buf.Count >= 2)
			{
				buf.RemoveAt(buf.Count - 1);
			}
			return buf.ToSqlString();
		}
	}
}
