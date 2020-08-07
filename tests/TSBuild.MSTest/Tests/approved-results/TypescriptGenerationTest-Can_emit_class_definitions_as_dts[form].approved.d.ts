interface LineItem {
	position?: number;
	name?: string;
}

interface Form {
	name?: string;
	items?: Array<LineItem>;
}
