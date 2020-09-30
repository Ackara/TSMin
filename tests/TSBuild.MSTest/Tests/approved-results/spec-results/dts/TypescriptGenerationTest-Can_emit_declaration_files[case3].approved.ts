interface Animal {
	name?: string;
	hitPoints?: number;
}

interface Tiger extends Animal {
	hasFangs?: boolean;
}
