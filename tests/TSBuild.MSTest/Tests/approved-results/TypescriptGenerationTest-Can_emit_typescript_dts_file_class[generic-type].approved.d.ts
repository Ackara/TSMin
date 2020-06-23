interface Predator {
	name?: string;
}

interface Skill<T> {
}

interface AfricanLion extends Predator, Skill<any> {
	region?: string;
}
