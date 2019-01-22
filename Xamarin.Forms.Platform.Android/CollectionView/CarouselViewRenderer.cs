using System.ComponentModel;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class CarouselViewRenderer : SelectableItemsViewRenderer
	{
		//should this move to ItemsViewREnderer and be shared ?
		class ScrollListener : global::Android.Support.V7.Widget.RecyclerView.OnScrollListener
		{
			CarouselViewRenderer _renderer;
			int _oldPosition;

			public ScrollListener(CarouselViewRenderer renderer)
			{
				_renderer = renderer;
			}

			public override void OnScrolled(global::Android.Support.V7.Widget.RecyclerView recyclerView, int dx, int dy)
			{
				var layoutManager = (recyclerView.GetLayoutManager() as LinearLayoutManager);
				var adapterPosition = layoutManager.FindFirstVisibleItemPosition();
				if (_oldPosition != adapterPosition)
				{
					_oldPosition = adapterPosition;
					_renderer.UpdatePosition(adapterPosition);
				}
				base.OnScrolled(recyclerView, dx, dy);
			}
		}

		// TODO hartez 2018/08/29 17:13:17 Does this need to override SelectLayout so it ignores grids?	(Yes, and so it can warn on unknown layouts)
		Context _context;
		ScrollListener _scrollListener;
		protected CarouselView Carousel;
		bool _isSwipeEnabled;

		public CarouselViewRenderer(Context context) : base(context)
		{
			_context = context;
			_scrollListener = new ScrollListener(this);
			AddOnScrollListener(_scrollListener);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_scrollListener != null)
				{
					RemoveOnScrollListener(_scrollListener);
					_scrollListener.Dispose();
					_scrollListener = null;
				}
			}
			base.Dispose(disposing);
		}

		protected override void SetUpNewElement(ItemsView newElement)
		{
			base.SetUpNewElement(newElement);
			if (newElement == null)
			{
				Carousel = null;
				return;
			}

			Carousel = newElement as CarouselView;

			UpdateSpacing();
			UpdateIsSwipeEnabled();

			//Goto to the Correct Position
			Carousel.ScrollTo(Carousel.Position);
		}

		protected override void UpdateItemsSource()
		{
			if (SelectableItemsView == null)
			{
				return;
			}

			// By default the CollectionViewAdapter creates the items at whatever size the template calls for
			// But for the Carousel, we want it to create the items to fit the width/height of the viewport
			// So we give it an alternate delegate for creating the views

			ItemsViewAdapter = new SelectableItemsViewAdapter(SelectableItemsView,
				(renderer, context) => new SizedItemContentView(renderer, context, () => Width, () => Height));

			SwapAdapter(ItemsViewAdapter, false);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == CarouselView.IsSwipeEnabledProperty.PropertyName)
				UpdateIsSwipeEnabled();
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			//TODO: this doesn't work because we need to interact with the Views
			if (!_isSwipeEnabled)
			{
				return false;
			}
			return base.OnTouchEvent(e);
		}

		void UpdateIsSwipeEnabled()
		{
			_isSwipeEnabled = Carousel.IsSwipeEnabled;
		}

		void UpdatePosition(int position)
		{
			if (position == -1)
				return;
			(ItemsViewAdapter as SelectableItemsViewAdapter)?.UpdateSelection(position);

			Carousel.SetValueCore(CarouselView.PositionProperty, position);
		}

		void UpdateSpacing()
		{

		}
	}
}