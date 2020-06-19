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

	interface Pathera extends Animal {
		stealth: number;
	}

	interface Lion extends Feline, Pathera {
	}


	interface Skill<T, Animal> {
		perform(arg: Animal): T;
	}


	interface AfricanLion extends Lion, Skill<any, Lion> {

	}
}



