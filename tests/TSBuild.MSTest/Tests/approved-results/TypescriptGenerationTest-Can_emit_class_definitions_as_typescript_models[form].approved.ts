namespace Foo {
	export class LineItemBase {
		position: number;
		name: string;
	}

	export class FormBase {
		name: string;
		items: Array<LineItemBase>;
	}
}
