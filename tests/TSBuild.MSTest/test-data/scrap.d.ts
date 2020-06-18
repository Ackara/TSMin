declare enum Foum {
	A,
	B,
	C,
}

declare namespace Foo {
	enum Status {
		Striving = 5,
		Endangered,
		Instinct,
	}

	interface Animal {
		name: string;
		legs: number;
		status: Status;
	}

	interface Feline extends Animal {
		whiskers: number;
	}

	class Lion implements Feline {
		whiskers: number;
		name: string;
		legs: number;
		status: Status;
		kills: number;
	}
}
