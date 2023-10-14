using Microsoft.EntityFrameworkCore;

namespace interview_test
{
	class PlanetDB : DbContext
	{
        public PlanetDB(DbContextOptions<PlanetDB> options)
                        : base(options) { }

        public DbSet<Planet> Planets => Set<Planet>();


        public PlanetDB()
		{
		}
    }
}

