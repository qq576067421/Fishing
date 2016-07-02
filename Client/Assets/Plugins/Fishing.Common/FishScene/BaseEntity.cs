using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public interface BaseEntity
    {
        bool IsDie { get; }
        bool IsDestroy { get; }
        int FishVibId { get; }
        int FishObjId { get; }
        EbVector3 Position { get; }
        void setDirection(float direction);
        void setPosition(EbVector3 position);
        void setSpeed(float speed);
        void setAngleSpeed(float angle_speed);
        void addRoute(IRoute route);
        void addDynamicSystem(DynamicSystem system);
        MassEntity getMassEntity();
        void update(float elapsed_tm);
        void destroy();
    }

    public interface BaseEntityFactory
    {
        string getFactoryName();
        BaseEntity buildBaseEntity(int vib_id, int fish_id);
        void destroyBaseEntity(BaseEntity entity);
    }
}
