interface Predator {
	name?: string;
}

interface Skill<any> {
}

interface AfricanLion extends Predator, Skill<any> {
	region?: string;
}
